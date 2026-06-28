using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 5f;
    
    [Header("Initial Loot")]
    public Autis.Inventory.ItemData[] possibleLoot;
    public int minLoot = 1;
    public int maxLoot = 1;

    [HideInInspector]
    public bool isOpen = false;

    // Internal
    private Animator _anim;

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        if (_anim != null)
        {
            _anim.enabled = false;
        }}



    public void OpenChest()
    {
        if (isOpen) return;
        isOpen = true;

        if (_anim != null) _anim.enabled = true;
        ParticleManager.SpawnChestOpen(transform.position + Vector3.up * 0.5f);
        
        // Spawn 1 random item
        if (possibleLoot != null && possibleLoot.Length > 0)
        {
            var lootData = possibleLoot[Random.Range(0, possibleLoot.Length)];
            if (lootData != null && lootData.worldPrefab != null)
            {
                // Spawn slightly above chest
                Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
                GameObject drop = Instantiate(lootData.worldPrefab, spawnPos, Quaternion.identity);
                
                // Make sure it floats beautifully by stripping physics
                WorldLootItem lootScript = drop.GetComponent<WorldLootItem>();
                if (lootScript == null) lootScript = drop.AddComponent<WorldLootItem>();
                lootScript.itemData = lootData;
                lootScript.quantity = 1;

                var oldDrop = drop.GetComponent<DropItem>();
                if (oldDrop != null) DestroyImmediate(oldDrop);
                var rb = drop.GetComponent<Rigidbody>();
                if (rb != null) DestroyImmediate(rb);
                var colliders = drop.GetComponentsInChildren<Collider>();
                foreach (var c in colliders) DestroyImmediate(c);
            }
        }
        
        // Chest remains for 45 seconds after FIRST opened, then disappears
        Destroy(gameObject, 45f);
    }
}
