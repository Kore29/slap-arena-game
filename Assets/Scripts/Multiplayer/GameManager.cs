using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject aiAgentPrefab;

    [Header("Game Mode Configuration")]
    public GameModeData currentModeData;
    public NetworkVariable<bool> isTeamMode = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public enum GameState { MainMenu, Lobby, Playing, Results }
    public GameState currentState = GameState.MainMenu;
    
    [Header("UI")]
    public GameHUD gameplayHUD;

    [Header("Environment")]
    public Transform arenaTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"<color=orange>⚠ Manager duplicado encontrado en {gameObject.name}. Borrando para evitar conflictos.</color>");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("<color=green>✔ GameManager Oficial Inicializado y Persistente.</color>");
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnLocalClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnLocalClientDisconnected;
        }
    }

    private void OnLocalClientDisconnected(ulong clientId)
    {
        // Si el que se desconecta somos nosotros (o el servidor cerró la conexión)
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("<color=orange>🌐 Local client disconnected from server.</color>");
            
            // Si estamos en medio de una partida o en resultados, volvemos al menú
            if (currentState == GameState.Playing || currentState == GameState.Results)
            {
                CleanupAndReturnToMenu();
            }
        }
    }


    /// <summary>
    /// Prepara la sesión de red (Lobby) sin cargar todavía la arena.
    /// </summary>
    public void InitializeNetworkedSession(bool isCoop)
    {
        if (currentModeData == null)
        {
            Debug.LogError("<color=red>🛑 CANNOT INITIALIZE: currentModeData is NULL in GameManager!</color>");
            return;
        }

        currentState = GameState.Lobby;
        
        if (NetworkManager.Singleton.IsHost)
        {
            SessionManager.Instance.InitializeLobby(currentModeData);
        }
        
        Debug.Log("<color=cyan>🎮 Session Initialized. Waiting in Lobby...</color>");
    }

    /// <summary>
    /// Carga la escena de la arena para todos los clientes. Solo ejecutable por el Server/Host.
    /// </summary>
    public void StartMatch()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Suscribirse a la carga de la escena de juego
        SceneManager.sceneLoaded -= OnNetworkedSceneLoaded;
        SceneManager.sceneLoaded += OnNetworkedSceneLoaded;

        NetworkManager.Singleton.SceneManager.LoadScene("PlatformArena", LoadSceneMode.Single);
        Debug.Log("<color=green>🌐 NetworkSceneManager: Loading PlatformArena for everyone.</color>");
    }

    [ClientRpc]
    private void SyncGameStateClientRpc(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] State synced to: {newState}");

        if (newState == GameState.Playing)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }

    private void OnNetworkedSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlatformArena")
        {
            SceneManager.sceneLoaded -= OnNetworkedSceneLoaded;
            
            // OCULTAR LOBBY (Tarea 1.1 - Fix Congelación)
            LobbyController lobby = Object.FindAnyObjectByType<LobbyController>();
            if (lobby != null)
            {
                lobby.SetVisibility(false);
                Debug.Log("<color=green>✔ Match Started: Lobby UI hidden.</color>");
            }

            if (NetworkManager.Singleton.IsServer)
            {
                currentState = GameState.Playing;
                SyncGameStateClientRpc(GameState.Playing);
                ExecuteNetworkedSpawning();
            }
        }
    }

    private void ExecuteNetworkedSpawning()
    {
        if (currentModeData == null)
        {
            Debug.LogError("<color=red>🛑 CRITICAL: currentModeData is NULL in GameManager during spawning!</color>");
            return;
        }

        Debug.Log("<color=cyan>🌐 Iniciando Spawning de Red...</color>");
        
        // Sincronizar el modo para los clientes
        isTeamMode.Value = currentModeData.isTeamBased;
        
        // --- ESCALADO DINÁMICO ---
        if (arenaTransform == null) arenaTransform = GameObject.Find("Arena")?.transform;
        if (arenaTransform != null && currentModeData != null)
        {
            arenaTransform.localScale = Vector3.one * currentModeData.arenaScale;
            Debug.Log($"<color=orange>Arena scaled to: {currentModeData.arenaScale}</color>");
        }
            
        // Buscar todos los puntos de spawn
        GameObject[] teamASpawns = GameObject.FindGameObjectsWithTag("SpawnPointA");
        GameObject[] teamBSpans = GameObject.FindGameObjectsWithTag("SpawnPointB");
        
        int aIdx = 0;
        int bIdx = 0;

        foreach (var slot in SessionManager.Instance.lobbySlots)
        {
            Transform spawnPoint = null;
            if (slot.TeamId == 0) spawnPoint = teamASpawns[aIdx++ % teamASpawns.Length].transform;
            else spawnPoint = teamBSpans[bIdx++ % teamBSpans.Length].transform;

            // --- SNAP TO FLOOR (Fix Vuelo Spawn) ---
            Vector3 finalSpawnPos = spawnPoint.position;
            // Raycast desde 5 metros arriba del spawn point hacia abajo
            if (Physics.Raycast(spawnPoint.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 15f))
            {
                finalSpawnPos = hit.point + Vector3.up * 0.1f; // Pequeño offset para evitar clipping con el suelo
                Debug.Log($"<color=green>✔ Adjusted spawn for {slot.Nickname} to floor at {finalSpawnPos.y}</color>");
            }

            if (slot.IsBot)
            {
                GameObject bot = Instantiate(aiAgentPrefab, finalSpawnPos, rotation: spawnPoint.rotation);
                NetworkObject nb = bot.GetComponent<NetworkObject>();
                nb.Spawn();
                
                // Configurar Equipo y Apodo
                TeamMember team = bot.GetComponent<TeamMember>();
                if (team != null) team.SetData(slot.TeamId, slot.Nickname);
            }
            else
            {
                GameObject player = Instantiate(playerPrefab, finalSpawnPos, rotation: spawnPoint.rotation);
                NetworkObject nb = player.GetComponent<NetworkObject>();
                nb.SpawnAsPlayerObject(slot.ClientId);

                // Configurar Equipo y Apodo
                TeamMember team = player.GetComponent<TeamMember>();
                if (team != null) team.SetData(slot.TeamId, slot.Nickname);
            }
        }
    }

    public void OnPlayerEliminated(TeamMember player)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (currentState == GameState.Results) return; // Ya terminó

        Debug.Log($"<color=orange>Player eliminated: {player.nickname.Value}</color>");
        
        // Comprobar si queda un ganador ignorando al que acaba de caer
        CheckMatchResults(player);
    }

    private void CheckMatchResults(TeamMember excludeMember = null)
    {
        TeamMember[] allPlayers = Object.FindObjectsByType<TeamMember>(FindObjectsInactive.Exclude);
        List<TeamMember> activePlayersList = new List<TeamMember>();
        
        foreach (var p in allPlayers)
        {
            if (p != excludeMember) activePlayersList.Add(p);
        }

        TeamMember[] activePlayers = activePlayersList.ToArray();
        
        if (currentModeData.isTeamBased)
        {
            HashSet<int> teamsLeft = new HashSet<int>();
            foreach (var p in activePlayers) teamsLeft.Add(p.teamId.Value);

            if (teamsLeft.Count <= 1)
            {
                int winnerTeam = teamsLeft.Count == 1 ? new List<int>(teamsLeft)[0] : -1;
                ulong winnerClientId = 999; // Default empty
                string winnerName = "DRAW";

                if (winnerTeam != -1)
                {
                    if (currentModeData.maxPlayers == 2 && activePlayers.Length == 1)
                    {
                        winnerName = activePlayers[0].nickname.Value.ToString().ToUpper() + " WINS!";
                        winnerClientId = activePlayers[0].OwnerClientId;
                    }
                    else
                    {
                        winnerName = (winnerTeam == 0 ? "TEAM ALPHA" : "TEAM BETA") + " WINS!";
                    }
                }

                ShowMatchResultsClientRpc(winnerName, winnerTeam, winnerClientId);
            }
        }
        else
        {
            if (activePlayers.Length <= 1)
            {
                string winnerName = activePlayers.Length == 1 ? activePlayers[0].nickname.Value.ToString() : "DRAW";
                ulong winnerClientId = activePlayers.Length == 1 ? activePlayers[0].OwnerClientId : 999;
                ShowMatchResultsClientRpc(winnerName, -1, winnerClientId);
            }
        }
    }

    [ClientRpc]
    private void ShowMatchResultsClientRpc(string winnerName, int winnerTeam, ulong winnerClientId)
    {
        SyncGameStateClientRpc(GameState.Results);
        Debug.Log($"<color=gold>🏆 MATCH OVER! Winner: {winnerName} (ID: {winnerClientId})</color>");
        
        // Aquí activaremos la UI de resultados (Tarea 4.3)
        MatchResultsController resultsUI = Object.FindAnyObjectByType<MatchResultsController>();
        if (resultsUI != null)
        {
            resultsUI.ShowResults(winnerName, winnerTeam, winnerClientId);
        }
    }

    /// <summary>
    /// Limpia todo el estado de red y juego y vuelve al menú principal.
    /// </summary>
    public void CleanupAndReturnToMenu()
    {
        Debug.Log("<color=yellow>🧹 Cleaning up game state and returning to Main Menu...</color>");
        
        // 1. Reset Session Data
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.ResetSession();
        }

        // 2. Reset GameManager state
        currentState = GameState.MainMenu;
        
        // 3. Ensure cursor is free
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        // 4. Shutdown Network (Importante para poder volver a Hostear/Unirse)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("<color=orange>🌐 NetworkManager Shutdown.</color>");
        }

        // 5. Load Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
