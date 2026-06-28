using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Autis.Inventory;
using TMPro;

namespace Autis.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject inventoryPanel;
        public Transform inventorySlotsContainer;
        public GameObject slotPrefab;

        [Header("Dragging")]
        public Image dragIcon;

        [Header("Tooltip")]
        public GameObject tooltipPanel;
        public TextMeshProUGUI tooltipText;

        private List<SlotUI> _allSlots = new List<SlotUI>();
        private int _selectedSlotIndex = -1;
        private SlotUI _draggedSlot;

        public static InventoryUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            if (dragIcon == null)
            {
                GameObject iconObj = new GameObject("DragIcon");
                iconObj.transform.SetParent(transform, false);
                iconObj.transform.SetAsLastSibling();
                dragIcon = iconObj.AddComponent<Image>();
                dragIcon.rectTransform.sizeDelta = new Vector2(64, 64);
            }

            if (dragIcon != null)
            {
                dragIcon.raycastTarget = false;
                dragIcon.gameObject.SetActive(false);
            }

            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (tooltipPanel != null) tooltipPanel.SetActive(false);

            // FIX: Disable raycastTarget on HotbarUI background so it
            // doesn't block drags from inventory to hotbar slots
            StartCoroutine(DisableHotbarRaycastBlocker());
        }

        // Wait one frame so HotbarUI has time to build itself first
        private System.Collections.IEnumerator DisableHotbarRaycastBlocker()
        {
            yield return null;

            HotbarUI hotbar = Object.FindFirstObjectByType<HotbarUI>();
            if (hotbar != null)
            {
                // Disable raycastTarget on the HotbarUI panel background image
                Image[] images = hotbar.GetComponentsInChildren<Image>(true);
                foreach (Image img in images)
                {
                    // Only disable background/container images, not slot icons
                    SlotUI slot = img.GetComponentInParent<SlotUI>();
                    if (slot == null)
                    {
                        img.raycastTarget = false;
                    }
                }
            }
        }

        public void RegisterSlot(SlotUI slot, int index)
        {
            slot.slotIndex = index;
            slot.parentUI = this;

            // Avoid duplicate registration
            if (!_allSlots.Contains(slot))
            {
                _allSlots.Add(slot);
            }
            _allSlots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
        }

        public void InitializeInventoryGrid(int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(slotPrefab, inventorySlotsContainer);
                SlotUI ui = obj.GetComponent<SlotUI>();
                RegisterSlot(ui, startIndex + i);
            }

            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            InventoryManager.Instance.OnHotbarIndexChanged += (idx) => RefreshUI();
            RefreshUI();
        }

        private void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.tabKey.wasPressedThisFrame)
                {
                    if (inventoryPanel != null)
                    {
                        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
                        var fpc = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();

                        if (!inventoryPanel.activeSelf)
                        {
                            HideTooltip();
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                            if (fpc != null) fpc.enabled = true;
                        }
                        else
                        {
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.visible = true;
                            if (fpc != null) fpc.enabled = false;
                        }
                    }
                }

                if (kb.qKey.wasPressedThisFrame && _selectedSlotIndex != -1)
                {
                    if (Camera.main != null)
                    {
                        InventoryManager.Instance.DropOneItemFromSlot(_selectedSlotIndex, Camera.main.transform);
                    }
                }
            }
        }

        public void RefreshUI()
        {
            var slots = InventoryManager.Instance.slots;
            foreach (var slotUI in _allSlots)
            {
                if (slotUI.slotIndex < slots.Count)
                {
                    bool isSelected = (slotUI.slotIndex == _selectedSlotIndex);
                    if (slotUI.slotIndex < InventoryManager.Instance.hotbarSlots &&
                        InventoryManager.Instance.activeHotbarIndex == slotUI.slotIndex)
                    {
                        isSelected = true;
                    }
                    slotUI.UpdateUI(slots[slotUI.slotIndex], isSelected);
                }
            }
        }

        public void SelectSlot(int index)
        {
            _selectedSlotIndex = index;
            RefreshUI();
        }

        // ── Drag logic ────────────────────────────────────────────────

        public void BeginDrag(SlotUI slot)
        {
            _draggedSlot = slot;
            if (dragIcon != null)
            {
                dragIcon.gameObject.SetActive(true);
                dragIcon.sprite = slot.currentSlotData.item.icon;

                var mouse = UnityEngine.InputSystem.Mouse.current;
                if (mouse != null)
                    dragIcon.transform.position = mouse.position.ReadValue();
            }
        }

        public void UpdateDrag(Vector2 pos)
        {
            if (_draggedSlot != null && dragIcon != null)
            {
                dragIcon.transform.position = pos;
            }
        }

        public void EndDrag(PointerEventData eventData)
        {
            if (_draggedSlot == null) return;
            if (dragIcon != null) dragIcon.gameObject.SetActive(false);

            SlotUI targetSlot = FindSlotUnderPointer(eventData);

            if (targetSlot != null && targetSlot != _draggedSlot)
            {
                int fromIdx = _draggedSlot.slotIndex;
                int toIdx   = targetSlot.slotIndex;
                Debug.Log($"[DragDrop] Swapping slot {fromIdx} with {toIdx}");
                if (fromIdx < 100 && toIdx < 100)
                {
                    InventoryManager.Instance.SwapSlots(fromIdx, toIdx);
                }
            }
            else
            {
                Debug.LogWarning("[DragDrop] No valid target slot found under pointer.");
            }

            _draggedSlot = null;
        }

        /// <summary>
        /// Tries multiple strategies to find which SlotUI is under the pointer.
        /// Strategy 1 – EventSystem.RaycastAll (works when raycastTarget is properly set)
        /// Strategy 2 – RectTransformUtility with null camera (Screen Space Overlay fix)
        /// Strategy 3 – Raw screen-point distance fallback
        /// </summary>
        private SlotUI FindSlotUnderPointer(PointerEventData eventData)
        {
            // ── Strategy 1: RaycastAll ──────────────────────────────
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var res in results)
            {
                if (res.gameObject == null) continue;
                SlotUI slot = res.gameObject.GetComponentInParent<SlotUI>();
                if (slot != null && slot != _draggedSlot)
                    return slot;
            }

            // ── Strategy 2: RectTransformUtility (null camera = overlay) ─
            Vector2 screenPos = eventData.position;
            foreach (var slot in _allSlots)
            {
                if (slot == _draggedSlot) continue;
                RectTransform rt = slot.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Pass null camera — required for Screen Space Overlay canvases
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null))
                    return slot;
            }

            // ── Strategy 3: Closest slot by screen distance ──────────
            // Only triggers if mouse is within 60px of a slot center
            SlotUI closest   = null;
            float  closestDist = 60f;
            foreach (var slot in _allSlots)
            {
                if (slot == _draggedSlot) continue;
                RectTransform rt = slot.GetComponent<RectTransform>();
                if (rt == null) continue;

                Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
                float dist = Vector2.Distance(screenPos, slotScreenPos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest     = slot;
                }
            }

            return closest;
        }

        // ── Tooltip ───────────────────────────────────────────────────

        public void ShowTooltip(ItemData item, Vector3 pos)
        {
            if (tooltipPanel != null && tooltipText != null)
            {
                tooltipPanel.SetActive(true);
                tooltipPanel.transform.position = pos + new Vector3(20, -20, 0);
                tooltipText.text = $"{item.itemName}\n<color=#aaaaaa>{item.type}</color>";
            }
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null) tooltipPanel.SetActive(false);
        }
    }
}
