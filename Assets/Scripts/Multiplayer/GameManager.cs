using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
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
    
    public enum GameState { MainMenu, Lobby, Playing, Results }
    public GameState currentState = GameState.MainMenu;
    
    [Header("UI")]
    public GameHUD gameplayHUD;

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

    public void StartNetworkedGame(bool isCoop)
    {
        currentState = GameState.Lobby;
        
        if (NetworkManager.Singleton.IsHost)
        {
            SessionManager.Instance.InitializeLobby(currentModeData);
        }

        // Suscribirse a la carga de la escena de juego
        SceneManager.sceneLoaded -= OnNetworkedSceneLoaded;
        SceneManager.sceneLoaded += OnNetworkedSceneLoaded;

        // Cargar escena para todos (via NetworkManager si estamos en sesión, pero aquí es inicio local antes del cambio)
        // Usaremos el NetworkSceneManager una vez estemos bien conectados
        SceneManager.LoadScene("PlatformArena");
    }

    private void OnNetworkedSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "PlatformArena")
        {
            SceneManager.sceneLoaded -= OnNetworkedSceneLoaded;
            if (NetworkManager.Singleton.IsServer)
            {
                ExecuteNetworkedSpawning();
            }
        }
    }

    private void ExecuteNetworkedSpawning()
    {
        Debug.Log("<color=cyan>🌐 Iniciando Spawning de Red...</color>");
        
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
                
                // Configurar Bot (Nickname, Equipo)
                // TODO: Sync bot data
            }
            else
            {
                // El player prefab se suele spawnear automáticamente por NGO si está en el NetworkManager, 
                // pero si queremos control total:
                GameObject player = Instantiate(playerPrefab, spawnPoint.position, rotation: spawnPoint.rotation);
                NetworkObject nb = player.GetComponent<NetworkObject>();
                nb.SpawnAsPlayerObject(slot.ClientId);
            }
        }
    }
}
