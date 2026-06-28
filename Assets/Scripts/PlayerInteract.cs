using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 5f;

    private Transform _cameraTransform;
    private float _lastHitTime = 0f;
    private float _hitCooldown = 0.4f;
    private string _debugTargetName = "Nothing";

    private void OnGUI()
    {
        GUI.color = Color.yellow;
        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(10, 10, 500, 50), "Looking at: " + _debugTargetName);
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (_cameraTransform == null) return;

        // Skip normal interactions if we are currently trying to place an item
        if (PlacementSystem.IsPlacementModeActive) return;

        // Raycast from center of screen every frame, ignoring the player's own collider
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactRange);
        
        GameObject lookedAtObject = null;
        float closestDistance = float.MaxValue;

        foreach (var h in hits)
        {
            if (h.collider.CompareTag("Player")) continue;
            
            if (h.distance < closestDistance)
            {
                closestDistance = h.distance;
                lookedAtObject = h.collider.gameObject;
            }
        }
        
        bool hitSomething = (lookedAtObject != null);
        _debugTargetName = hitSomething ? lookedAtObject.name : "Nothing";

        // Reset UI by default
        bool showPrompt = false;
        string promptMessage = "";

        // Handle interactions based on what we are looking at
        if (lookedAtObject != null)
        {
            DropItem drop = lookedAtObject.GetComponentInParent<DropItem>();
            if (drop != null)
            {
                showPrompt = true;
                promptMessage = "PRESS E TO COLLECT";
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                {
                    drop.Pickup();
                }
            }

            Chest chest = lookedAtObject.GetComponentInParent<Chest>();
            if (chest != null && !chest.isOpen)
            {
                showPrompt = true;
                promptMessage = "RIGHT CLICK TO OPEN";
                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
                {
                    chest.OpenChest();
                }
            }
        }

        // --- AGGRESSIVE DEBUGGING ---
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (lookedAtObject == null) Debug.LogWarning("[DEBUG] Right clicked, but Raycast hit NOTHING within 5 units!");
            else Debug.LogWarning("[DEBUG] Right clicked! Raycast is hitting: " + lookedAtObject.name + " on Layer: " + LayerMask.LayerToName(lookedAtObject.layer));
        }

        // Update UI
        if (Autis.UI.InteractionUI.Instance != null)
        {
            if (showPrompt) Autis.UI.InteractionUI.Instance.ShowPrompt(promptMessage);
            else Autis.UI.InteractionUI.Instance.HidePrompt();
        }

        if (Autis.Inventory.InventoryManager.Instance != null)
        {
            var eq = Autis.Inventory.InventoryManager.Instance.GetEquippedItem();
            _hitCooldown = (eq != null) ? eq.attackCooldown : 0.5f;
        }

        // Left Click to attack trees/rocks
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - _lastHitTime >= _hitCooldown && hitSomething)
            {
                _lastHitTime = Time.time;
                HandleHit(lookedAtObject);
            }
        }
    }

    private void HandleHit(GameObject hitObj)
    {
        Autis.Inventory.ItemData equipped = null;
        
        if (Autis.Inventory.InventoryManager.Instance != null)
        {
            equipped = Autis.Inventory.InventoryManager.Instance.GetEquippedItem();
        }

        Animator anim = GetComponentInParent<Animator>();
        if (anim != null) anim.SetTrigger("Attack");

        float damage = 10f; // Bare hands default
        if (equipped != null && equipped.damageValue > 0)
        {
            damage = equipped.damageValue;
        }

        // --- ENEMY INTERACTION ---
        EnemyHealth enemy = hitObj.GetComponentInParent<EnemyHealth>();
        if (enemy != null && !enemy.isDead)
        {
            enemy.TakeDamage(damage);

            AudioSource audio = GetComponentInParent<AudioSource>();
            if (audio != null) audio.Play();

            return;
        }

        // --- TREE INTERACTION ---
        TreeHealth tree = hitObj.GetComponentInParent<TreeHealth>();
        if (tree != null)
        {
            float treeDmg = damage * 0.3f; // non-axe reduction
            if (equipped != null && equipped.itemName.ToLower().Contains("axe") && !equipped.itemName.ToLower().Contains("pickaxe"))
            {
                treeDmg = damage; // Full damage for axe
            }
            
            tree.TakeDamage(Mathf.RoundToInt(treeDmg));
            return;
        }

        // --- ROCK INTERACTION ---
        RockNode rock = hitObj.GetComponentInParent<RockNode>();
        if (rock != null)
        {
            bool isPickaxe = (equipped != null && (equipped.itemName.ToLower().Contains("pickaxe") || equipped.itemName.ToLower().Contains("stone tool")));

            if (isPickaxe)
            {
                rock.TakeDamage(Mathf.RoundToInt(damage), true);
            }
            else
            {
                Autis.UI.HintUI.Show("Need a Pickaxe!");
            }
            return;
        }
    }
}
