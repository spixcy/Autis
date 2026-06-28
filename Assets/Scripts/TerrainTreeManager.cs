using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Attach this script to your Player.
/// It detects painted Terrain trees, tracks their health, and when destroyed,
/// removes them from the Terrain and spawns a real falling GameObject in their place.
/// </summary>
public class TerrainTreeManager : MonoBehaviour
{
    [Header("Terrain Setup")]
    [Tooltip("The terrain with your painted trees. If left blank, it will find the active terrain.")]
    public Terrain targetTerrain;


    [Header("Hit Settings")]
    public float hitRange = 1.2f;
    public float hitCooldown = 1.0f;
    public int maxTreeHealth = 3;

    [Header("Audio")]
    public AudioClip hitSound;

    // Internal tracking
    private float lastHitTime = -999f;
    private Dictionary<Vector3Int, int> treeHealthMap = new Dictionary<Vector3Int, int>();
    private AudioSource sfx;
    private PlayerHealth playerHealth;

    // Cache the trees so we don't cause lag by reading from the Terrain every frame
    private TerrainData tData;
    private TreeInstance[] cachedTrees;

    void Start()
    {
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;
        if (targetTerrain != null)
        {
            tData = targetTerrain.terrainData;
            cachedTrees = tData.treeInstances;
        }
        else
        {
            Debug.LogWarning("[TerrainTreeManager] Could not find an active Terrain!");
        }

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.spatialBlend = 1f;

        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (tData == null || cachedTrees == null) return;
        
        // Only try to hit a tree if the player clicks the left mouse button
        if (UnityEngine.InputSystem.Mouse.current == null || !UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

        if (Time.time - lastHitTime < hitCooldown) return;

        Vector3 playerPos = transform.position;
        Vector3 terrainPos = targetTerrain.transform.position;
        Vector3 terrainSize = tData.size;

        for (int i = 0; i < cachedTrees.Length; i++)
        {
            Vector3 worldPos = Vector3.Scale(cachedTrees[i].position, terrainSize) + terrainPos;

            // Fast square bounding box check first to save CPU
            if (Mathf.Abs(playerPos.x - worldPos.x) > hitRange) continue;
            if (Mathf.Abs(playerPos.z - worldPos.z) > hitRange) continue;

            // Accurate circle distance check
            Vector3 delta = playerPos - worldPos;
            delta.y = 0;
            if (delta.sqrMagnitude <= hitRange * hitRange)
            {
                HitTree(i, worldPos);
                break; // Only hit one tree per tick
            }
        }
    }

    private void HitTree(int treeIndex, Vector3 worldPos)
    {
        lastHitTime = Time.time;

        if (hitSound != null) sfx.PlayOneShot(hitSound);

        // Generate a unique ID for this tree using its local position
        Vector3 localPos = cachedTrees[treeIndex].position;
        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(localPos.x * 100000f),
            Mathf.RoundToInt(localPos.y * 100000f),
            Mathf.RoundToInt(localPos.z * 100000f)
        );

        int currentHp = maxTreeHealth;
        if (treeHealthMap.ContainsKey(key))
        {
            currentHp = treeHealthMap[key];
        }

        currentHp--;
        Debug.Log($"[TerrainTreeManager] Hit terrain tree! HP: {currentHp}/{maxTreeHealth}");

        if (currentHp <= 0)
        {
            // The tree is dead! 
            int prototypeIndex = cachedTrees[treeIndex].prototypeIndex;
            float treeRot = cachedTrees[treeIndex].rotation;
            float treeScaleW = cachedTrees[treeIndex].widthScale;
            float treeScaleH = cachedTrees[treeIndex].heightScale;

            // 1. Remove it from the Terrain
            List<TreeInstance> treeList = new List<TreeInstance>(cachedTrees);
            treeList.RemoveAt(treeIndex);
            
            cachedTrees = treeList.ToArray();
            tData.treeInstances = cachedTrees; // This permanently updates the Terrain!

            treeHealthMap.Remove(key); // Cleanup dictionary

            // 2. Spawn the real GameObject in its place AUTOMATICALLY
            TreePrototype[] prototypes = tData.treePrototypes;
            if (prototypeIndex >= 0 && prototypeIndex < prototypes.Length)
            {
                GameObject prefab = prototypes[prototypeIndex].prefab;
                if (prefab != null)
                {
                    GameObject realTree = Instantiate(prefab, worldPos, Quaternion.Euler(0, treeRot * Mathf.Rad2Deg, 0));
                    
                    // Match the random scale the terrain gave it
                    realTree.transform.localScale = new Vector3(treeScaleW, treeScaleH, treeScaleW);

                    // 3. Immediately trigger the fall animation
                    TreePhysics tp = realTree.GetComponent<TreePhysics>();
                    
                    // If the user forgot to add the script to their prefab, we will add it for them magically!
                    if (tp == null)
                    {
                        tp = realTree.AddComponent<TreePhysics>();
                    }

                    tp.ForceFall();
                }
                else
                {
                    Debug.LogWarning($"[TerrainTreeManager] The tree prototype at index {prototypeIndex} does not have a prefab assigned in the Terrain!");
                }
            }
        }
        else
        {
            // Tree is hurt but still alive
            treeHealthMap[key] = currentHp;
            
            // Optional: You could spawn a particle effect here!
        }
    }
}
