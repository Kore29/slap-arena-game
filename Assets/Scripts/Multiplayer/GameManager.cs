using Unity.Netcode;
using UnityEngine;

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
        
        // Spawn Local Player
        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            
            // Spawn AI Agent
            GameObject ai = Instantiate(aiAgentPrefab, enemySpawnPoint.position, Quaternion.identity);
            
            // Connect them (the AI needs to know who its target is)
            var agent = ai.GetComponent<EnemyAgent>();
            if (agent != null) agent.targetOpponent = player.transform;

            // Inicializar HUD (Punto 3.2)
            if (gameplayHUD != null)
            {
                gameplayHUD.Initialize(player.GetComponent<PlayerController>());
            }
        }
        else
        {
            Debug.LogError("PlayerPrefab is missing in GameManager!");
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
