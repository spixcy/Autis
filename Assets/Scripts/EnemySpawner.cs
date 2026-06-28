using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public int maxEnemies = 10;
    public float spawnRadius = 40f;
    public float minDistanceFromPlayer = 15f;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Transform player;
    private float spawnTimer;

    private void Start()
    {
        var pc = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();
        if (pc != null) player = pc.transform;

        // Initial spawn
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (activeEnemies.Count < maxEnemies)
        {
            spawnTimer += Time.deltaTime;
            // Respawn every 30s as requested
            if (spawnTimer >= 30f)
            {
                spawnTimer = 0f;
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        Vector3 spawnPos = Vector3.zero;
        bool foundPos = false;

        float mapWidth = 500f;
        float mapLength = 500f;
        Vector3 terrainPos = Vector3.zero;

        if (Terrain.activeTerrain != null)
        {
            mapWidth = Terrain.activeTerrain.terrainData.size.x;
            mapLength = Terrain.activeTerrain.terrainData.size.z;
            terrainPos = Terrain.activeTerrain.transform.position;
        }

        for (int i = 0; i < 30; i++)
        {
            float randX = Random.Range(terrainPos.x, terrainPos.x + mapWidth);
            float randZ = Random.Range(terrainPos.z, terrainPos.z + mapLength);
            Vector3 testPos = new Vector3(randX, 200f, randZ);

            if (player != null)
            {
                float dist = Vector2.Distance(new Vector2(testPos.x, testPos.z), new Vector2(player.position.x, player.position.z));
                if (dist < minDistanceFromPlayer) continue;
            }

            if (Physics.Raycast(testPos, Vector3.down, out RaycastHit hit, 300f))
            {
                spawnPos = hit.point;
                foundPos = true;
                break;
            }
        }

        if (foundPos)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(enemy);
        }
    }
}
