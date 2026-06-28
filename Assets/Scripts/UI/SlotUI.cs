using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Autis.Inventory;

namespace Autis.UI
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image iconImage;
        public TextMeshProUGUI quantityText;
        public GameObject highlightBorder;

        [HideInInspector] public int slotIndex;
        [HideInInspector] public ItemSlot currentSlotData;
        
        [HideInInspector] public InventoryUI parentUI;

        private void Awake()
        {
            // Force raycast targets to ensure drag and drop works perfectly
            Image bg = GetComponent<Image>();
            if (bg != null) bg.raycastTarget = true;

            if (iconImage != null) iconImage.raycastTarget = false;
            if (quantityText != null) quantityText.raycastTarget = false;
            
            if (highlightBorder != null)
            {
                Image hImg = highlightBorder.GetComponent<Image>();
                if (hImg != null) hImg.raycastTarget = false;
            }
        }

        public void UpdateUI(ItemSlot slotData, bool isSelected)
        {
            currentSlotData = slotData;
            
            if (slotData == null || slotData.IsEmpty)
            {
                if (iconImage != null) iconImage.gameObject.SetActive(false);
                if (quantityText != null) quantityText.gameObject.SetActive(false);
            }
            else
            {
                if (iconImage != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = slotData.item.icon;
                }
                
                if (quantityText != null)
                {
                    if (slotData.item.isStackable && slotData.quantity > 1)
                    {
                        quantityText.gameObject.SetActive(true);
                        quantityText.text = slotData.quantity.ToString();
                    }
                    else
                    {
                        quantityText.gameObject.SetActive(false);
                    }
                }
            }

            if (highlightBorder != null)
            {
                highlightBorder.SetActive(isSelected);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (parentUI != null) parentUI.SelectSlot(slotIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentSlotData == null || currentSlotData.IsEmpty) return;
            if (parentUI != null) parentUI.BeginDrag(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (parentUI != null) parentUI.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (parentUI != null) parentUI.EndDrag(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentSlotData != null && !currentSlotData.IsEmpty)
            {
                if (parentUI != null) parentUI.ShowTooltip(currentSlotData.item, transform.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (parentUI != null) parentUI.HideTooltip();
        }
    }
}
