using UnityEngine;

/// <summary>
/// Attach to the Player GameObject (same one that has FirstPersonController).
/// Detects when the CharacterController hits a tree by looking for
/// TreePhysics anywhere in the hit object's hierarchy — no tag needed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerTreeCollision : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("How much damage to deal to the TREE each collision.")]
    public int damageToTree = 1;

    [Tooltip("Minimum horizontal speed (m/s) the player needs to be moving\n" +
             "to register a collision. Prevents standing against a tree from\n" +
             "spamming damage.")]
    public float minImpactSpeed = 0.5f;

    // ── cached refs ───────────────────────────────────────────────────
    private CharacterController _cc;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _cc           = GetComponent<CharacterController>();
        _playerHealth = GetComponent<PlayerHealth>();
    }

    /// <summary>
    /// Unity calls this every frame the CharacterController is touching
    /// another collider. We walk UP the hit object's hierarchy to find
    /// a TreePhysics component — works regardless of which child the
    /// collider belongs to.
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Skip collisions from below (e.g. stepping on a log)
        if (hit.moveDirection.y < -0.3f) return;

        // Must be moving fast enough to count as an "impact"
        float hSpeed = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z).magnitude;
        if (hSpeed < minImpactSpeed) return;

        // Walk up hierarchy to find TreePhysics (handles LOD child objects)
        TreePhysics tree = hit.collider.GetComponentInParent<TreePhysics>();
        if (tree == null) return;   // not a tree — ignore

        // ── Hit the tree ──────────────────────────────────────────────
        tree.TakeDamage(damageToTree);

        // ── Hurt the player ───────────────────────────────────────────
        if (_playerHealth != null)
            _playerHealth.TryDamage(_playerHealth.treeDamage);

        Debug.Log($"[TreeCollision] Hit '{tree.name}' HP={tree.CurrentHealth}");
    }
}
