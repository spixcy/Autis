using UnityEngine;
using Autis.Inventory;

public class HeldItemController : MonoBehaviour
{
    [SerializeField] private Transform handPosition;

    private GameObject _currentHeldObject;
    private ItemData _currentItemData;

    // Sway variables
    private Vector3 _initialLocalPos;
    
    // Mouse follow variables
    private float smoothSpeed = 8f;
    private float swayAmplitude = 0.02f;
    private float swaySpeed = 1.2f;

    /*
      SETUP IN EDITOR:
      1. Select MainCamera child hierarchy
      2. Create empty GameObject -> name it "HandPosition"
      3. Set local pos: X 0.4  Y -0.3  Z 0.6
      4. Set local rot: X 0    Y -30   Z 0
      5. Attach HeldItemController.cs to MainCamera
      6. Drag HandPosition into the handPosition slot
      7. On MainCamera -> Culling Mask -> make sure it includes Default layer
    */

    private void Start()
    {
        if (handPosition != null)
        {
            _initialLocalPos = handPosition.localPosition;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquippedItemChanged += HandleEquippedItemChanged;
            // Trigger it once at start
            HandleEquippedItemChanged(InventoryManager.Instance.currentEquipped);
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnEquippedItemChanged -= HandleEquippedItemChanged;
        }
    }

    private void HandleEquippedItemChanged(ItemData newItem)
    {
        if (newItem == _currentItemData && _currentHeldObject != null) return;
        _currentItemData = newItem;

        // Clean up old
        if (_currentHeldObject != null)
        {
            Destroy(_currentHeldObject);
            _currentHeldObject = null;
        }

        // Spawn new
        if (newItem != null && newItem.worldPrefab != null && handPosition != null)
        {
            _currentHeldObject = Instantiate(newItem.worldPrefab, handPosition);
            _currentHeldObject.transform.localPosition = Vector3.zero;
            _currentHeldObject.transform.localRotation = Quaternion.identity;
            _currentHeldObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Strip physics and interaction scripts BEFORE colliders to avoid RequireComponent errors
            var loot = _currentHeldObject.GetComponent<WorldLootItem>();
            if (loot != null) DestroyImmediate(loot);

            var drop = _currentHeldObject.GetComponent<DropItem>();
            if (drop != null) DestroyImmediate(drop);

            var rb = _currentHeldObject.GetComponent<Rigidbody>();
            if (rb != null) DestroyImmediate(rb);

            var colliders = _currentHeldObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                DestroyImmediate(col);
            }
        }
    }

    private void Update()
    {
        if (handPosition == null) return;

        // Mouse lag/follow
        float mouseX = 0f;
        float mouseY = 0f;

        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            mouseX = UnityEngine.InputSystem.Mouse.current.delta.x.ReadValue() * -0.005f;
            mouseY = UnityEngine.InputSystem.Mouse.current.delta.y.ReadValue() * -0.005f;
        }

        // Clamp values
        mouseX = Mathf.Clamp(mouseX, -0.1f, 0.1f);
        mouseY = Mathf.Clamp(mouseY, -0.1f, 0.1f);

        // Idle Sway
        float swayX = Mathf.Sin(Time.time * swaySpeed) * swayAmplitude;
        float swayY = Mathf.Cos(Time.time * swaySpeed * 2f) * swayAmplitude;

        Vector3 finalPos = _initialLocalPos + new Vector3(mouseX + swayX, mouseY + swayY, 0);
        handPosition.localPosition = Vector3.Lerp(handPosition.localPosition, finalPos, Time.deltaTime * smoothSpeed);
    }
}
