using System.Collections.Generic;
using UnityEngine;
using System;

namespace Autis.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        public int totalSlots = 24;
        public int hotbarSlots = 5;

        public List<ItemSlot> slots = new List<ItemSlot>();
        
        public int activeHotbarIndex = 0;

        public event Action OnInventoryChanged;
        public event Action<int> OnHotbarIndexChanged;
        
        public ItemData currentEquipped;
        public event Action<ItemData> OnEquippedItemChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Initialize empty slots
            for (int i = 0; i < totalSlots; i++)
            {
                slots.Add(new ItemSlot());
            }

            OnInventoryChanged += RefreshEquippedItem;
        }

        public void SetEquippedItem(ItemData item)
        {
            if (currentEquipped != item)
            {
                currentEquipped = item;
                OnEquippedItemChanged?.Invoke(currentEquipped);
            }
        }

        private void RefreshEquippedItem()
        {
            SetEquippedItem(GetEquippedItem());
        }

        private void Update()
        {
            HandleHotbarInput();
        }

        private void HandleHotbarInput()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) SetActiveHotbarIndex(0);
            if (kb.digit2Key.wasPressedThisFrame) SetActiveHotbarIndex(1);
            if (kb.digit3Key.wasPressedThisFrame) SetActiveHotbarIndex(2);
            if (kb.digit4Key.wasPressedThisFrame) SetActiveHotbarIndex(3);
            if (kb.digit5Key.wasPressedThisFrame) SetActiveHotbarIndex(4);

            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll > 0)
                {
                    int newIdx = activeHotbarIndex - 1;
                    if (newIdx < 0) newIdx = hotbarSlots - 1;
                    SetActiveHotbarIndex(newIdx);
                }
                else if (scroll < 0)
                {
                    int newIdx = activeHotbarIndex + 1;
                    if (newIdx >= hotbarSlots) newIdx = 0;
                    SetActiveHotbarIndex(newIdx);
                }
            }
        }

        public void SetActiveHotbarIndex(int index)
        {
            if (index >= 0 && index < hotbarSlots && index != activeHotbarIndex)
            {
                activeHotbarIndex = index;
                OnHotbarIndexChanged?.Invoke(activeHotbarIndex);
                OnInventoryChanged?.Invoke(); // UI might need refresh to show glow
            }
        }

        public ItemData GetEquippedItem()
        {
            if (activeHotbarIndex >= 0 && activeHotbarIndex < slots.Count)
            {
                return slots[activeHotbarIndex].item;
            }
            return null;
        }

        public ItemSlot GetEquippedSlot()
        {
            if (activeHotbarIndex >= 0 && activeHotbarIndex < slots.Count)
            {
                return slots[activeHotbarIndex];
            }
            return null;
        }

        public void ConsumeEquippedItem()
        {
            var slot = GetEquippedSlot();
            if (slot != null && !slot.IsEmpty)
            {
                slot.quantity--;
                if (slot.quantity <= 0) slot.Clear();
                OnInventoryChanged?.Invoke();
            }
        }

        // Returns how many items were actually added (in case inventory fills up)
        public int AddItem(ItemData itemData, int quantity)
        {
            int remaining = quantity;

            // 1. Try to add to existing stacks
            if (itemData.isStackable)
            {
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty && slot.item == itemData && slot.quantity < itemData.maxStack)
                    {
                        int space = itemData.maxStack - slot.quantity;
                        if (remaining <= space)
                        {
                            slot.quantity += remaining;
                            remaining = 0;
                            break;
                        }
                        else
                        {
                            slot.quantity += space;
                            remaining -= space;
                        }
                    }
                }
            }

            // 2. Try to find empty slots
            if (remaining > 0)
            {
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty)
                    {
                        slot.item = itemData;
                        
                        if (remaining <= itemData.maxStack)
                        {
                            slot.quantity = remaining;
                            remaining = 0;
                            break;
                        }
                        else
                        {
                            slot.quantity = itemData.maxStack;
                            remaining -= itemData.maxStack;
                        }
                    }
                }
            }

            int added = quantity - remaining;
            if (added > 0)
            {
                OnInventoryChanged?.Invoke();
            }

            return remaining; // return what couldn't be added
        }

        public bool HasItem(ItemData itemData, int amount = 1)
        {
            int found = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == itemData)
                {
                    found += slot.quantity;
                    if (found >= amount) return true;
                }
            }
            return false;
        }

        public void RemoveItem(ItemData itemData, int amount)
        {
            int remainingToRemove = amount;
            // Iterate backwards to remove from bottom-right slots first
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.item == itemData)
                {
                    if (slot.quantity >= remainingToRemove)
                    {
                        slot.quantity -= remainingToRemove;
                        if (slot.quantity == 0) slot.Clear();
                        remainingToRemove = 0;
                        break;
                    }
                    else
                    {
                        remainingToRemove -= slot.quantity;
                        slot.Clear();
                    }
                }
            }
            
            OnInventoryChanged?.Invoke();
        }

        public void DropOneItemFromSlot(int index, Transform dropPoint)
        {
            if (index < 0 || index >= slots.Count) return;
            var slot = slots[index];
            if (slot.IsEmpty || slot.item.worldPrefab == null) return;

            // Spawn physically
            GameObject drop = Instantiate(slot.item.worldPrefab, dropPoint.position + dropPoint.forward * 1.5f + Vector3.up, Quaternion.identity);
            
            // Re-apply DropItem script
            DropItem dropScript = drop.GetComponent<DropItem>();
            if (dropScript == null) dropScript = drop.AddComponent<DropItem>();
            dropScript.itemData = slot.item;
            dropScript.quantity = 1;

            slot.quantity -= 1;
            if (slot.quantity <= 0) slot.Clear();

            OnInventoryChanged?.Invoke();
        }

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= slots.Count || indexB < 0 || indexB >= slots.Count) return;
            
            var tempItem = slots[indexA].item;
            var tempQty = slots[indexA].quantity;

            slots[indexA].item = slots[indexB].item;
            slots[indexA].quantity = slots[indexB].quantity;

            slots[indexB].item = tempItem;
            slots[indexB].quantity = tempQty;

            OnInventoryChanged?.Invoke();
        }
    }
}
