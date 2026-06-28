using UnityEngine;
using UnityEngine.UI;
using Autis.Inventory;
using TMPro;

public class WorldLootItem : MonoBehaviour
{
    public ItemData itemData;
    public int quantity = 1;

    private float _startY;
    private Transform _playerCam;
    private GameObject _uiCanvasObj;

    private void Start()
    {
        _startY = transform.position.y;
        if (Camera.main != null) _playerCam = Camera.main.transform;

        // Create UI Canvas programmatically
        _uiCanvasObj = new GameObject("LootPromptCanvas");
        _uiCanvasObj.transform.SetParent(transform);
        _uiCanvasObj.transform.localPosition = new Vector3(0, 1.2f, 0); // Hover above item
        
        Canvas canvas = _uiCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform rt = _uiCanvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);
        rt.localScale = new Vector3(0.01f, 0.01f, 0.01f); // Scale down for world space

        // Add Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(_uiCanvasObj.transform, false);
        TextMeshProUGUI promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press E";
        promptText.fontSize = 24;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        
        // Hide by default
        _uiCanvasObj.SetActive(false);
    }

    private void Update()
    {
        // 1. Bob up and down (sine wave, amplitude 0.3f, speed 2f)
        float newY = _startY + Mathf.Sin(Time.time * 2f) * 0.3f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 2. Rotate slowly on Y axis (60 deg/sec)
        transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);

        if (_playerCam != null)
        {
            float dist = Vector3.Distance(transform.position, _playerCam.position);
            bool inRange = dist <= 3f;
            
            if (_uiCanvasObj.activeSelf != inRange)
            {
                _uiCanvasObj.SetActive(inRange);
            }

            // 3. Collect when in range and pressing E
            if (inRange)
            {
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    CollectItem();
                }
            }
        }
    }

    private void LateUpdate()
    {
        // 4. Billboard effect - always face camera exactly
        if (_uiCanvasObj.activeSelf && _playerCam != null)
        {
            _uiCanvasObj.transform.rotation = Quaternion.LookRotation(_uiCanvasObj.transform.position - _playerCam.position);
        }
    }

    private void CollectItem()
    {
        if (InventoryManager.Instance != null && itemData != null)
        {
            int leftover = InventoryManager.Instance.AddItem(itemData, quantity);
            if (leftover < quantity) // Picked up some or all
            {
                // Play Sparkle
                ParticleManager.SpawnPickup(transform.position);
                
                if (leftover == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    quantity = leftover;
                }
            }
        }
    }
}
