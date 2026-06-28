using UnityEngine;

/*
  HOW TO SET UP A DROP PREFAB:
  1. Create a GameObject with your mesh
  2. Add Rigidbody (Use Gravity ON, drag = 1)
  3. Add SphereCollider (Is Trigger ON, radius 0.8)
  4. Add DropItem.cs
  5. Assign ItemData ScriptableObject in Inspector
  6. Set quantity
  7. Save as prefab in Assets/Prefabs/Drops/
  8. Assign prefab to TreeHealth or RockNode in Inspector
*/

[RequireComponent(typeof(SphereCollider))]
public class DropItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public Autis.Inventory.ItemData itemData;
    public int quantity = 1;

    [Header("Animation Settings")]
    public float bobAmplitude = 0.1f;
    public float bobSpeed = 1.5f;
    public float spinSpeed = 45f;

    private float _startY;

    private void Start()
    {
        // Setup collider
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.8f;

        _startY = transform.position.y;
    }

    private void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);

        Vector3 pos = transform.position;
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmplitude * Time.deltaTime;
        transform.position = pos;
    }

    // We removed OnTriggerEnter because we now use the crosshair + E key to pick up items manually!

    public void Pickup()
    {
        if (itemData != null && Autis.Inventory.InventoryManager.Instance != null)
        {
            int remaining = Autis.Inventory.InventoryManager.Instance.AddItem(itemData, quantity);
            
            if (remaining < quantity)
            {
                // We picked up at least something
                ParticleManager.SpawnPickup(transform.position);
                
                if (remaining <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    quantity = remaining; // Leave the rest on the floor
                }
            }
        }
    }
}
