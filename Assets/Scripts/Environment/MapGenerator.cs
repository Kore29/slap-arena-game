using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapGenerator : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] grassPrefabs;

    [Header("Sync")]
    public NetworkVariable<int> syncedSeed = new NetworkVariable<int>(0);
    public NetworkVariable<float> syncedScale = new NetworkVariable<float>(1.0f);

    public override void OnNetworkSpawn()
    {
        Debug.Log($"<color=white>[MapGenerator] 📶 OnNetworkSpawn called! IsServer: {IsServer}, Seed: {syncedSeed.Value}</color>");

        // Suscribirse a cambios (Para Clientes)
        syncedSeed.OnValueChanged += (oldVal, newVal) => {
            Debug.Log($"<color=cyan>[MapGenerator] Seed changed from {oldVal} to {newVal}. Generating...</color>");
            CleanAndGenerate(newVal, syncedScale.Value);
        };
        syncedScale.OnValueChanged += (oldVal, newVal) => {
            Debug.Log($"<color=cyan>[MapGenerator] Scale changed to {newVal}. Generating...</color>");
            CleanAndGenerate(syncedSeed.Value, newVal);
        };

        if (IsServer)
        {
            StartCoroutine(ServerSyncRoutine());
        }
        else 
        {
            // Si somos cliente y la semilla ya tiene valor (entramos tarde), generamos
            if (syncedSeed.Value != 0) 
            {
                Debug.Log($"<color=cyan>[MapGenerator] Client late-join generation with Seed: {syncedSeed.Value}</color>");
                CleanAndGenerate(syncedSeed.Value, syncedScale.Value);
            }
        }
    }

    private System.Collections.IEnumerator ServerSyncRoutine()
    {
        Debug.Log("<color=white>[MapGenerator] ⚙ ServerSyncRoutine started. Synchronizing with GameManager...</color>");
        
        float timeout = 5f; 
        while (timeout > 0)
        {
            if (GameManager.Instance != null && GameManager.Instance.currentModeData != null)
            {
                float targetScale = GameManager.Instance.currentModeData.arenaScale;
                int targetSeed = Random.Range(1, 999999);

                Debug.Log($"<color=green>[MapGenerator] Sync SUCCESS. Mode: {GameManager.Instance.currentModeData.name} | Scale: {targetScale} | Seed: {targetSeed}</color>");

                // Sincronizar hacia los clientes
                syncedScale.Value = targetScale;
                syncedSeed.Value = targetSeed;

                // --- IMPORTANTE: El Host genera inmediatamente ---
                // No esperamos al evento OnValueChanged (que a veces se pierde o retrasa en el propio Host)
                CleanAndGenerate(targetSeed, targetScale);
                
                yield break;
            }
            
            timeout -= 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogError("<color=red>[MapGenerator] ❌ FATAL: ServerSyncRoutine timed out! Ensure currentModeData is assigned in GameManager.</color>");
    }

    [Header("Spawn Settings")]
    [SerializeField] private int minTrees = 1;
    [SerializeField] private int maxTrees = 2;
    [SerializeField] private int minRocks = 3;
    [SerializeField] private int maxRocks = 4;
    [SerializeField] private int grassDensity = 50;
    [SerializeField] private float globalScaleMultiplier = 2f;
    
    [Header("Area Settings")]
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float exclusionRadius = 3f;
    [SerializeField] private List<Transform> exclusionPoints = new List<Transform>();

    [Header("Runtime")]
    public int currentSeed;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    /// <summary>
    /// Generates a new environment based on a seed.
    /// </summary>
    /// <param name="seed">The integer seed for deterministic generation.</param>
    public void CleanAndGenerate(int seed, float scale = 1.0f)
    {
        currentSeed = seed;
        Random.InitState(seed);
        
        // AUTO-CONFIG: Si no hay puntos de exclusión, buscarlos por tag
        if (exclusionPoints == null || exclusionPoints.Count == 0)
        {
            GameObject[] a = GameObject.FindGameObjectsWithTag("SpawnPointA");
            GameObject[] b = GameObject.FindGameObjectsWithTag("SpawnPointB");
            foreach(var g in a) exclusionPoints.Add(g.transform);
            foreach(var g in b) exclusionPoints.Add(g.transform);
        }

        // Ajustar radio de spawn dinámicamente según la escala
        float adjustedRadius = spawnRadius * scale;
        
        ClearExistingObjects();
        
        // 1. Generate Trees
        int treeCount = Random.Range(minTrees, maxTrees + 1);
        SpawnObjects(treePrefabs, treeCount, "Tree", false, adjustedRadius);

        // 2. Generate Rocks
        int rockCount = Random.Range(minRocks, maxRocks + 1);
        SpawnObjects(rockPrefabs, rockCount, "Rock", false, adjustedRadius);

        // 3. Generate Grass (always)
        SpawnObjects(grassPrefabs, grassDensity, "Grass", true, adjustedRadius);
        
        Debug.Log($"<color=green>🌳 Map Generated with seed: {seed} and scale: {scale} (Radius: {adjustedRadius}) | Objects: {spawnedObjects.Count}</color>");
        if (spawnedObjects.Count == 0) Debug.LogWarning("<color=orange>⚠ No objects spawned! Check GroundLayer and Raycast settings.</color>");
    }

    private void ClearExistingObjects()
    {
        // For runtime
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // For editor cleanup
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            // Evitar destruir otros componentes si los hubiera
            if (transform.GetChild(i).gameObject.name != "MapGenerator") 
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnObjects(GameObject[] prefabs, int count, string category, bool isGrass = false, float overrideRadius = -1f)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        float radius = overrideRadius > 0 ? overrideRadius : spawnRadius;
        int attempts = 0;
        int spawned = 0;

        while (spawned < count && attempts < count * 8) // Un poco más de margen
        {
            attempts++;
            
            // Random point in circle
            Vector2 randomPoint = Random.insideUnitCircle * radius;
            Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 50f, randomPoint.y);

            // Check exclusion zones
            if (!isGrass && IsInExclusionZone(new Vector3(spawnPos.x, transform.position.y, spawnPos.z))) continue;

            // Raycast down to surface (Ignorando TRIGGERS como la zona de muerte)
            RaycastHit hit;
            bool groundFound = Physics.Raycast(spawnPos, Vector3.down, out hit, 100f, groundLayer, QueryTriggerInteraction.Ignore);
            if (!groundFound) groundFound = Physics.Raycast(spawnPos, Vector3.down, out hit, 100f, ~0, QueryTriggerInteraction.Ignore);

            if (groundFound)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject instance = Instantiate(prefab, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0), transform);
                
                // --- AJUSTES DINÁMICOS ---
                
                // 1. Escala (Global x Random)
                float scaleMod = Random.Range(0.85f, 1.15f) * globalScaleMultiplier;
                instance.transform.localScale *= scaleMod;
                
                // 2. Colisiones (Solo Árboles tienen colisión)
                if (category == "Rock" || category == "Grass")
                {
                    Collider col = instance.GetComponentInChildren<Collider>();
                    if (col != null) col.enabled = false; 
                }
                
                spawnedObjects.Add(instance);
                spawned++;
            }
        }
    }

    public void SetExclusionPoints(List<Transform> points)
    {
        exclusionPoints = points;
    }

    private bool IsInExclusionZone(Vector3 pos)
    {
        foreach (var point in exclusionPoints)
        {
            if (point == null) continue;
            if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(point.position.x, 0, point.position.z)) < exclusionRadius)
            {
                return true;
            }
        }
        return false;
    }

    [ContextMenu("Generate Randomly")]
    public void GenerateRandomly()
    {
        CleanAndGenerate(Random.Range(0, 99999));
    }
}
