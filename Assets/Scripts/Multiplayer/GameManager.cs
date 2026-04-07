using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject aiAgentPrefab;

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    public enum GameMode { LocalPractice, PVP_Online, Coop_Online }
    public GameMode currentMode;
    
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
        
        // El Manager principal mandará a través de toda la partida
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("<color=green>✔ GameManager Oficial Inicializado y Persistente.</color>");
    }

    public void StartLocalGame()
    {
        currentMode = GameMode.LocalPractice;
        Debug.Log("<color=cyan>🎮 Iniciando Modo Práctica Offline...</color>");
        
        // LIMPIEZA PREVENTIVA: Nos aseguramos de no estar suscritos dos veces
        SceneManager.sceneLoaded -= OnPracticeSceneLoaded;
        SceneManager.sceneLoaded += OnPracticeSceneLoaded;
        
        SceneManager.LoadScene("PlatformArena");
    }

    private void OnPracticeSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"<color=cyan>📦 Escena Cargada: {scene.name}. Procediendo al Spawning...</color>");
        if (scene.name == "PlatformArena")
        {
            SceneManager.sceneLoaded -= OnPracticeSceneLoaded;
            ExecuteSpawning();
        }
    }

    private void ExecuteSpawning()
    {
        // LIMPIEZA: Borrar clones antiguos inmediatamente
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in oldPlayers) {
            p.tag = "Untagged"; // Evitar que se encuentren en el siguiente frame
            DestroyImmediate(p);
        }
        
        EnemyAgent[] oldEnemies = Object.FindObjectsByType<EnemyAgent>(FindObjectsInactive.Include);
        foreach (var e in oldEnemies) DestroyImmediate(e.gameObject);
        
        // BIND: Reconectar las referencias que se perdieron al cambiar de escena
        if (playerSpawnPoint == null) playerSpawnPoint = GameObject.Find("PlayerSpawn")?.transform;
        if (enemySpawnPoint == null) enemySpawnPoint = GameObject.Find("EnemySpawn")?.transform;
        if (gameplayHUD == null) gameplayHUD = Object.FindAnyObjectByType<GameHUD>();
        
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

            // Inicializar HUD
            if (gameplayHUD != null)
            {
                gameplayHUD.Initialize(player.GetComponent<PlayerController>());
            }
        }
        else
        {
            Debug.LogError("Setup fallido: Revisa los SpawnPoints en la escena PlatformArena.");
        }
    }

    // This would be called from the SessionManager callbacks or NetworkManager events
    public void StartNetworkedGame(bool isCoop)
    {
        currentMode = isCoop ? GameMode.Coop_Online : GameMode.PVP_Online;
        
        // In Netcode, the NetworkManager handles spawning of "Player Prefab".
        // But for Coop AIs, only the Server/Host should spawn them.
        if (NetworkManager.Singleton.IsServer)
        {
            if (isCoop)
            {
                // Spawn AI agents as NetworkObjects
                GameObject ai = Instantiate(aiAgentPrefab, enemySpawnPoint.position, Quaternion.identity);
                ai.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
