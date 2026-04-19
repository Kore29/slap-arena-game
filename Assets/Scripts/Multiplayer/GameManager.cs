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

    [Header("Spawn Points (Old)")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

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

    public void StartLocalGame()
    {
        currentState = GameState.Playing;
        Debug.Log("<color=cyan>🎮 Iniciando Modo Práctica Offline...</color>");
        
        SceneManager.sceneLoaded -= OnPracticeSceneLoaded;
        SceneManager.sceneLoaded += OnPracticeSceneLoaded;
        
        SceneManager.LoadScene("PlatformArena");
    }

    private void OnPracticeSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlatformArena")
        {
            SceneManager.sceneLoaded -= OnPracticeSceneLoaded;
            ExecuteSpawning();
        }
    }

    private void ExecuteSpawning()
    {
        // Limpieza de objetos antiguos
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in oldPlayers) DestroyImmediate(p);
        
        // BIND: Reconectar las referencias que se perdieron al cambiar de escena
        if (playerSpawnPoint == null) playerSpawnPoint = GameObject.Find("PlayerSpawn")?.transform;
        if (enemySpawnPoint == null) enemySpawnPoint = GameObject.Find("EnemySpawn")?.transform;
        if (gameplayHUD == null) gameplayHUD = Object.FindAnyObjectByType<GameHUD>();
        
        // --- AMBIENTE DINÁMICO ---
        MapGenerator mapDecorator = Object.FindAnyObjectByType<MapGenerator>();
        if (mapDecorator != null)
        {
            var excl = new System.Collections.Generic.List<Transform>();
            if (playerSpawnPoint != null) excl.Add(playerSpawnPoint);
            if (enemySpawnPoint != null) excl.Add(enemySpawnPoint);
            
            mapDecorator.SetExclusionPoints(excl);
            mapDecorator.CleanAndGenerate(Random.Range(0, 999999));
        }
        
        // Spawn Local Player
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            
            // Spawn AI Agent
            if (aiAgentPrefab != null && enemySpawnPoint != null)
            {
                GameObject ai = Instantiate(aiAgentPrefab, enemySpawnPoint.position, Quaternion.identity);
                // Connect them
                var agent = ai.GetComponent<EnemyAgent>();
                if (agent != null) agent.targetOpponent = player.transform;
            }

            if (gameplayHUD != null) gameplayHUD.Initialize(player.GetComponent<PlayerController>());
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

            if (slot.IsBot)
            {
                GameObject bot = Instantiate(aiAgentPrefab, spawnPoint.position, rotation: spawnPoint.rotation);
                NetworkObject nb = bot.GetComponent<NetworkObject>();
                nb.Spawn();
                
                // Configurar Equipo y Apodo
                TeamMember team = bot.GetComponent<TeamMember>();
                if (team != null) team.SetData(slot.TeamId, slot.Nickname);
            }
            else
            {
                GameObject player = Instantiate(playerPrefab, spawnPoint.position, rotation: spawnPoint.rotation);
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

        Debug.Log($"<color=orange>Player eliminated: {player.nickname.Value}</color>");
        
        // Comprobar si queda un ganador
        CheckMatchResults();
    }

    private void CheckMatchResults()
    {
        TeamMember[] activePlayers = Object.FindObjectsByType<TeamMember>(FindObjectsInactive.Exclude);
        
        if (currentModeData.isTeamBased)
        {
            HashSet<int> teamsLeft = new HashSet<int>();
            foreach (var p in activePlayers) teamsLeft.Add(p.teamId.Value);

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
                        // In team mode, we don't need a single winnerClientId, but we set it to a neutral high value
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
}
