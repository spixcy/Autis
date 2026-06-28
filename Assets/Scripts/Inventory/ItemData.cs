using UnityEngine;

namespace Autis.Inventory
{
    public enum ItemType { Resource, Tool, Weapon, Placeable }

    [CreateAssetMenu(fileName = "New Item Data", menuName = "Autis/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public Sprite icon;
        public GameObject worldPrefab;
        public bool isStackable;
        public int maxStack = 99;
        public ItemType type;
        
        [Header("Stats")]
        [Tooltip("Damage value for tools hitting trees/rocks, or weapons hitting enemies")]
        public int damageValue;
        public float useRange = 2.5f;
        public float attackCooldown = 0.5f;
        public string itemCategory;

        [Header("Gathering Multipliers")]
        public float treeDamageMultiplier = 1f;
        public float rockDamageMultiplier = 1f;

        /*
          HOW TO SET UP AN ITEM:
          1. Create a 2D PNG icon (128x128 recommended, transparent background)
             Import it -> Texture Type: Sprite (2D and UI)
          2. Create a 3D prefab of the item mesh
             Add Rigidbody + SphereCollider (trigger) + DropItem.cs to it
             Save to Assets/Prefabs/Drops/
          3. Create ItemData: Right-click Assets/Items -> Create -> Autis -> Item Data
          4. Fill in name, assign icon Sprite, assign worldPrefab, set type and values
          5. Assign this ItemData to DropItem.cs on the world prefab
        */
    }
}
