using UnityEngine;
using System.Collections.Generic;

public class PlacementSystem : MonoBehaviour
{
    [Header("Settings")]
    public float placementDistance = 2.5f;
    public Material ghostMaterialValid;
    public Material ghostMaterialInvalid; // Optional, if we do bounds checking later
    public LayerMask placementLayer;

    private bool _isPlacing = false;
    private Autis.Inventory.ItemData _itemToPlace;
    private GameObject _ghostObject;
    private Transform _cameraTransform;
    
    // So PlayerInteract can know not to swing an axe
    public static bool IsPlacementModeActive { get; private set; }

    private void Start()
    {
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
        IsPlacementModeActive = false;
    }

    private void Update()
    {
        if (_cameraTransform == null) return;

        // Toggle Placement Mode
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (!_isPlacing)
            {
                TryStartPlacement();
            }
            else
            {
                CancelPlacement();
            }
        }

        if (_isPlacing)
        {
            HandlePlacementMode();
        }
    }

    private void TryStartPlacement()
    {
        if (Autis.Inventory.InventoryManager.Instance == null) return;
        
        var item = Autis.Inventory.InventoryManager.Instance.GetEquippedItem();
        
        // Only allow placing items marked as Placeable
        if (item == null || item.worldPrefab == null || item.type != Autis.Inventory.ItemType.Placeable)
        {
            return;
        }

        _itemToPlace = item;
        _isPlacing = true;
        IsPlacementModeActive = true;

        // Create Ghost
        _ghostObject = Instantiate(item.worldPrefab);
        
        // Strip physics/scripts from ghost
        Collider[] colliders = _ghostObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders) Destroy(col);
        
        MonoBehaviour[] scripts = _ghostObject.GetComponentsInChildren<MonoBehaviour>();
        foreach (var s in scripts) Destroy(s);

        Rigidbody rb = _ghostObject.GetComponentInChildren<Rigidbody>();
        if (rb != null) Destroy(rb);

        // Apply ghost material
        if (ghostMaterialValid != null)
        {
            MeshRenderer[] renderers = _ghostObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                Material[] mats = new Material[r.materials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = ghostMaterialValid;
                r.materials = mats;
            }
        }
    }

    private void HandlePlacementMode()
    {
        // Cancel on Escape or Right Click
        bool escPressed = UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
        bool rmbPressed = UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame;
        
        if (escPressed || rmbPressed)
        {
            CancelPlacement();
            return;
        }

        // Raycast logic to find ground
        Vector3 targetPoint = _cameraTransform.position + _cameraTransform.forward * placementDistance;
        Ray ray = new Ray(targetPoint + Vector3.up * 2f, Vector3.down);
        
        bool isValid = false;
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, placementLayer))
        {
            _ghostObject.transform.position = hit.point;
            _ghostObject.transform.up = hit.normal; // Align to slope
            isValid = true;
        }
        else
        {
            // Just float in front if ground not found directly below
            _ghostObject.transform.position = targetPoint;
            _ghostObject.transform.up = Vector3.up;
        }

        _ghostObject.SetActive(isValid);

        // Confirm Placement on Left Click
        if (isValid && UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            ConfirmPlacement(hit.point, hit.normal);
        }
    }

    private void ConfirmPlacement(Vector3 pos, Vector3 normal)
    {
        if (Autis.Inventory.InventoryManager.Instance == null) return;
        
        var slot = Autis.Inventory.InventoryManager.Instance.GetEquippedSlot();
        if (slot != null && slot.item == _itemToPlace && slot.quantity > 0)
        {
            Autis.Inventory.InventoryManager.Instance.ConsumeEquippedItem();

            // Spawn actual prefab
            GameObject placedObject = Instantiate(_itemToPlace.worldPrefab, pos, Quaternion.identity);
            placedObject.transform.up = normal;

            // Ensure DropItem is attached so they can pick it up
            DropItem drop = placedObject.GetComponent<DropItem>();
            if (drop == null)
            {
                drop = placedObject.AddComponent<DropItem>();
                drop.itemData = _itemToPlace;
                drop.quantity = 1;
            }

            ParticleManager.SpawnPickup(pos);
        }

        CancelPlacement();
    }

    private void CancelPlacement()
    {
        _isPlacing = false;
        IsPlacementModeActive = false;
        _itemToPlace = null;
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
        }
    }


}
