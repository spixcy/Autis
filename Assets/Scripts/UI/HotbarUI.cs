using UnityEngine;
using Autis.Inventory;

namespace Autis.UI
{
    public class HotbarUI : MonoBehaviour
    {
        public Transform hotbarContainer;
        public GameObject slotPrefab;

        private void Start()
        {
            int hotbarCount = InventoryManager.Instance.hotbarSlots;
            
            for (int i = 0; i < hotbarCount; i++)
            {
                GameObject obj = Instantiate(slotPrefab, hotbarContainer);
                SlotUI ui = obj.GetComponent<SlotUI>();
                InventoryUI.Instance.RegisterSlot(ui, i);
            }

            // Tell the main inventory to spawn the rest (indices 5 to 23)
            int remaining = InventoryManager.Instance.totalSlots - hotbarCount;
            InventoryUI.Instance.InitializeInventoryGrid(hotbarCount, remaining);
        }

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnHotbarIndexChanged += HandleHotbarChanged;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnHotbarIndexChanged -= HandleHotbarChanged;
        }

        private void HandleHotbarChanged(int index)
        {
            if (InventoryManager.Instance != null)
            {
                ItemData item = InventoryManager.Instance.GetEquippedItem();
                InventoryManager.Instance.SetEquippedItem(item);
            }
        }
    }
}
