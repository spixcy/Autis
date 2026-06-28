using UnityEngine;

/// <summary>
/// Attach to a tree root GameObject.
///
/// Detection method: distance check in Update() — 100% reliable,
/// no Physics callbacks, no Rigidbody, no tag required.
///
/// Solid CapsuleCollider (to block the player) must exist on this
/// GameObject with IsTrigger = FALSE. The script will warn you if
/// it can't find one.
/// </summary>
public class TreePhysics : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Hit Detection")]
    [Tooltip("Horizontal distance (metres) at which the tree registers a hit.")]
    public float hitRange = 1.2f;
    [Tooltip("Seconds between repeated damage ticks while the player is close.")]
    public float hitCooldown = 1.0f;

    [Header("Fall Settings")]
    public float fallDuration = 1.5f;
    [Tooltip("Degrees the tree rotates when falling. 90 = flat on ground.")]
    public float fallAngle = 90f;
    [Tooltip("Local-space axis the tree falls along. " +
             "Watch the yellow arrow in Scene view and adjust until it looks right.")]
    public Vector3 fallAxis = Vector3.right;

    [Header("Audio (optional — drag clips here)")]
    public AudioClip hitSound;
    public AudioClip fallSound;

    // ── Public read-only ──────────────────────────────────────────────
    public int  CurrentHealth => _hp;
    public bool IsFalling     => _isFalling;
    public bool HasFallen     => _hasFallen;

    // ── Private runtime ────────────────────────────────────────────────
    private int        _hp;
    private bool       _isFalling;
    private bool       _hasFallen;
    private float      _fallTimer;
    private Quaternion _startRot;
    private Quaternion _targetRot;
    private float      _lastHitTime = -999f;

    private Transform    _playerTf;
    private PlayerHealth _playerHealth;
    private AudioSource  _sfx;

    // ── Unity lifecycle ────────────────────────────────────────────────
    private void Awake()
    {
        // ── Audio ──────────────────────────────────────────────────────
        _sfx = GetComponent<AudioSource>();
        if (_sfx == null) _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.spatialBlend = 1f;
        _sfx.playOnAwake  = false;
    }

    private void Start()
    {
        _hp = maxHealth;

        // ── Find the player ────────────────────────────────────────────
        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (_playerHealth != null)
        {
            _playerTf = _playerHealth.transform;
            Debug.Log($"[TreePhysics] '{name}' found player: {_playerTf.name}");
        }
        else
        {
            Debug.LogWarning($"[TreePhysics] '{name}' could NOT find PlayerHealth " +
                             "in the scene! Make sure PlayerHealth is attached to the Player.");
        }

        // ── Collider sanity check ──────────────────────────────────────
        bool hasSolid = false;
        foreach (Collider c in GetComponents<Collider>())
        {
            if (!c.isTrigger) { hasSolid = true; break; }
        }
        if (!hasSolid)
        {
            Debug.LogWarning($"[TreePhysics] '{name}' has NO solid (non-trigger) Collider! " +
                             "Player will walk through it. Add a CapsuleCollider with " +
                             "Is Trigger = OFF.");
        }
    }

    private void Update()
    {
        // ── Fall animation ─────────────────────────────────────────────
        if (_isFalling)
        {
            _fallTimer += Time.deltaTime;
            float t     = Mathf.Clamp01(_fallTimer / fallDuration);
            float eased = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);   // ease-in
            transform.rotation = Quaternion.Lerp(_startRot, _targetRot, eased);

            if (_fallTimer >= fallDuration)
            {
                transform.rotation = _targetRot;
                _isFalling         = false;
                _hasFallen         = true;

                // Disable all colliders once fallen
                foreach (Collider c in GetComponents<Collider>())
                    c.enabled = false;

                Destroy(gameObject, 8f);
            }
            return; // skip hit detection while falling
        }

        if (_hasFallen) return;

        // ── Distance-based hit detection ───────────────────────────────
        if (_playerTf == null) return;
        
        // Only try to chop if the player clicks the left mouse button
        if (UnityEngine.InputSystem.Mouse.current == null || !UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

        // Only measure horizontal distance (ignore height difference)
        Vector3 delta = _playerTf.position - transform.position;
        delta.y       = 0f;
        float dist    = delta.magnitude;

        if (dist <= hitRange)
        {
            if (Time.time - _lastHitTime >= hitCooldown)
            {
                _lastHitTime = Time.time;
                Debug.Log($"[TreePhysics] Player chopped '{name}'! " +
                          $"HP: {_hp - 1}/{maxHealth}");
                TakeDamage(1);
                // Removed the code that damages the player here
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────
    public void ForceFall()
    {
        if (_isFalling || _hasFallen) return;
        _hp = 0;
        BeginFall();
    }

    public void TakeDamage(int amount = 1)
    {
        if (_isFalling || _hasFallen) return;

        _hp -= amount;

        if (hitSound != null) _sfx.PlayOneShot(hitSound);
        StartCoroutine(WobbleCo());

        if (_hp <= 0) BeginFall();
    }

    // ── Private helpers ────────────────────────────────────────────────
    private void BeginFall()
    {
        _isFalling = true;
        _fallTimer = 0f;
        _startRot  = transform.rotation;

        Vector3 worldAxis = transform.TransformDirection(fallAxis.normalized);
        _targetRot = Quaternion.AngleAxis(fallAngle, worldAxis) * _startRot;

        if (fallSound != null) _sfx.PlayOneShot(fallSound);

        Debug.Log($"[TreePhysics] '{name}' is FALLING!");
    }

    private System.Collections.IEnumerator WobbleCo()
    {
        float elapsed  = 0f;
        const float dur = 0.3f;
        Quaternion orig = transform.localRotation;

        while (elapsed < dur)
        {
            if (_isFalling) yield break;
            elapsed += Time.deltaTime;
            float a = Mathf.Sin((elapsed / dur) * Mathf.PI * 4f) * 3f;
            transform.localRotation = orig * Quaternion.Euler(0f, 0f, a);
            yield return null;
        }

        if (!_isFalling)
            transform.localRotation = orig;
    }

    // ── Scene-view gizmo ───────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Hit range circle
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, hitRange);

        // Fall direction arrow
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + Vector3.up * 2f,
                       transform.TransformDirection(fallAxis.normalized) * 5f);

        // Label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 9f,
            $"Hit range: {hitRange}m");
#endif
    }
#endif
}
