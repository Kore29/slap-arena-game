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
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartLocalGame()
    {
        currentMode = GameMode.LocalPractice;
        
        // RE-CARREGA: Cargamos la escena y esperamos a que termine para spawnear (Punto 2)
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
        // LIMPIEZA: Borrar clones antiguos para evitar la "explosión de muñecos" (Punto 2)
        GameObject[] oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in oldPlayers) Destroy(p);
        
        EnemyAgent[] oldEnemies = Object.FindObjectsByType<EnemyAgent>(FindObjectsSortMode.None);
        foreach (var e in oldEnemies) Destroy(e.gameObject);
        
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
