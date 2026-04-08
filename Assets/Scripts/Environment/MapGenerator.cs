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

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host/Server decides the seed if it hasn't been set
            if (syncedSeed.Value == 0)
                syncedSeed.Value = Random.Range(1, 999999);
        }

        // Everyone generates when they join and see the seed
        CleanAndGenerate(syncedSeed.Value);
        
        // Also listen for changes (though the seed shouldn't change mid-match usually)
        syncedSeed.OnValueChanged += (oldVal, newVal) => {
            CleanAndGenerate(newVal);
        };
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
    public void CleanAndGenerate(int seed)
    {
        currentSeed = seed;
        Random.InitState(seed);
        
        ClearExistingObjects();
        
        // 1. Generate Trees
        int treeCount = Random.Range(minTrees, maxTrees + 1);
        SpawnObjects(treePrefabs, treeCount, "Tree");

        // 2. Generate Rocks
        int rockCount = Random.Range(minRocks, maxRocks + 1);
        SpawnObjects(rockPrefabs, rockCount, "Rock");

        // 3. Generate Grass (always)
        SpawnObjects(grassPrefabs, grassDensity, "Grass", true);
        
        Debug.Log($"<color=green>🌳 Map Generated with seed: {seed}</color>");
    }

    private void ClearExistingObjects()
    {
        // For runtime
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // For editor cleanup (if objects were spawned as children manually)
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnObjects(GameObject[] prefabs, int count, string category, bool isGrass = false)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        int attempts = 0;
        int spawned = 0;

        while (spawned < count && attempts < count * 5)
        {
            attempts++;
            
            // Random point in circle
            Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 20f, randomPoint.y);

            // Check exclusion zones (don't spawn trees/rocks on spawn points)
            if (!isGrass && IsInExclusionZone(new Vector3(spawnPos.x, transform.position.y, spawnPos.z)))
            {
                continue;
            }

            // Raycast down to surface
            if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 40f, groundLayer))
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
