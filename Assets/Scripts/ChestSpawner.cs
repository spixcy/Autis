    using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [Header("Chest Setup")]
    public GameObject[] chestPrefabs;
    
    [Header("Spawn Settings")]
    public int numberOfChests = 20;
    
    [Tooltip("If left blank, it will automatically find the active terrain.")]
    public Terrain targetTerrain;
    
    [Tooltip("The bounds where chests can spawn. X is width, Z is length.")]
    public Vector2 spawnAreaSize = new Vector2(500f, 500f);

    void Start()
    {
        SpawnAllChests();
    }

    [ContextMenu("Spawn Chests Now")]
    public void SpawnAllChests()
    {
        if (chestPrefabs == null || chestPrefabs.Length == 0)
        {
            Debug.LogError("[ChestSpawner] Please assign at least one chest prefab to the array!");
            return;
        }

        if (targetTerrain == null)
        {
            targetTerrain = Terrain.activeTerrain;
            if (targetTerrain == null)
            {
                Debug.LogError("[ChestSpawner] Could not find an active terrain!");
                return;
            }
        }

        Vector3 terrainPos = targetTerrain.transform.position;

        for (int i = 0; i < numberOfChests; i++)
        {
            // Pick a random X and Z within the area
            float randomX = Random.Range(0, spawnAreaSize.x);
            float randomZ = Random.Range(0, spawnAreaSize.y);
            
            // Calculate world position (X and Z only)
            Vector3 worldPos = terrainPos + new Vector3(randomX, 0, randomZ);
            
            // Get the exact terrain height at this X and Z
            float y = targetTerrain.SampleHeight(worldPos);
            worldPos.y = y + terrainPos.y;

            // Pick a random color chest from the array
            GameObject randomPrefab = chestPrefabs[Random.Range(0, chestPrefabs.Length)];

            // Random rotation so they don't all face the same way
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360f), 0);

            // Spawn it!
            Instantiate(randomPrefab, worldPos, randomRot, transform);
        }
        
        Debug.Log($"[ChestSpawner] Successfully scattered {numberOfChests} chests across the map!");
    }
}
