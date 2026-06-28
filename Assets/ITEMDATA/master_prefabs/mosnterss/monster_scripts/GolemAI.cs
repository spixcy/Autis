using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Autis.Inventory;

/// <summary>
/// Golem-specific AI. Uses EnemyHealth.cs for HP and damage events.
/// Attach this to the Golem prefab — NavMeshAgent, EnemyHealth, and a Rigidbody
/// (for touch damage) are auto-required / expected.
///
/// ANIMATION STATES USED (exact names from CT_Golem_Sample animator):
///   SA_Golem_Idle    — looping idle
///   SA_Golem_Walk    — looping walk (used for both patrol and chase)
///   SA_Golem_Hammer  — one-shot attack
///   SA_Golem_Down    — one-shot death (freezes on last frame)
///
/// NOTE: SA_Golem_Damage (hurt reaction) is intentionally NOT used anymore.
/// Getting hit no longer stuns the golem or interrupts its current action —
/// it just plays a hit sound and keeps attacking/chasing with no cooldown.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class GolemAI : MonoBehaviour
{
    // ── Animation state name constants ───────────────────────────────
    private const string ANIM_IDLE   = "SA_Golem_Idle";
    private const string ANIM_WALK   = "SA_Golem_Walk";
    private const string ANIM_ATTACK = "SA_Golem_Hammer";
    private const string ANIM_DEATH  = "SA_Golem_Down";

    // ── State machine ────────────────────────────────────────────────
    // NOTE: Hurt state removed — taking damage no longer changes state.
    private enum State { Idle, Wander, Chase, Attack, Death }
    private State currentState = State.Idle;

    // ── Inspector settings ───────────────────────────────────────────
    [Header("Detection")]
    [SerializeField] private float detectionRange  = 12f;
    [SerializeField] private float attackRange     = 2.5f;
    [SerializeField] private float losePlayerTime  = 3f;   // seconds before giving up chase

    [Header("Movement")]
    [SerializeField] private float wanderSpeed     = 2f;
    [SerializeField] private float chaseSpeed      = 4.5f;
    [SerializeField] private float wanderRadius    = 8f;

    [Header("Combat - Hammer Attack")]
    [SerializeField] private float attackDamage      = 20f;
    [SerializeField] private float attackDamageDelay = 0.6f; // animation wind-up before damage lands
    [SerializeField] private float attackCycleTime   = 1f;   // how long the attack swing "occupies" the golem before it can swing again
    // NOTE: there is intentionally no cooldown beyond attackCycleTime — as soon as
    // the current swing finishes, if the player is still in range, it swings again immediately.

    [Header("Combat - Touch Damage")]
    [Tooltip("Damage dealt just from the player's body touching the golem's collider/rigidbody.")]
    [SerializeField] private float touchDamage         = 5f;
    [Tooltip("Minimum time between touch-damage ticks while the player stays in contact.")]
    [SerializeField] private float touchDamageInterval = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioSource audioSource; // auto-added if left empty

    [Header("Loot on Death")]
    [SerializeField] private ItemData[] dropTable;
    [SerializeField] private int   dropMin       = 1;
    [SerializeField] private int   dropMax       = 3;
    [Tooltip("Outward horizontal force applied to drops so they skid/drag away from the death point.")]
    [SerializeField] private float dropOutwardForce = 6f;
    [Tooltip("Upward pop force applied to drops.")]
    [SerializeField] private float dropUpwardForce  = 3f;
    [Tooltip("Linear drag set on dropped items so they slide a noticeable distance before stopping.")]
    [SerializeField] private float dropLinearDrag   = 0.3f;

    // ── Private references ───────────────────────────────────────────
    private NavMeshAgent  agent;
    private Animator      anim;
    private EnemyHealth   health;
    private Transform     player;
    private PlayerHealth  playerHealth;

    // ── Runtime state ────────────────────────────────────────────────
    private float stateTimer;
    private float timeSinceLastSeen;
    private bool  playerInSight;
    private float lastTouchDamageTime = -99f;

    // ════════════════════════════════════════════════════════════════
    //  Unity lifecycle
    // ════════════════════════════════════════════════════════════════

    private void Start()
    {
        agent  = GetComponent<NavMeshAgent>();
        anim   = GetComponentInChildren<Animator>();
        health = GetComponent<EnemyHealth>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Subscribe to EnemyHealth events
        health.OnDeath      += HandleDeath;
        health.OnTakeDamage += HandleTakeDamage;

        // Auto-find player via StarterAssets FirstPersonController
        var fpc = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();
        if (fpc != null)
        {
            player       = fpc.transform;
            playerHealth = fpc.GetComponentInParent<PlayerHealth>()
                        ?? fpc.GetComponent<PlayerHealth>();
        }

        EnterIdle();
    }

    private void Update()
    {
        if (health.isDead) return;

        UpdateLineOfSight();

        switch (currentState)
        {
            case State.Idle:   TickIdle();   break;
            case State.Wander: TickWander(); break;
            case State.Chase:  TickChase();  break;
            case State.Attack: TickAttack(); break;
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Touch damage — player takes damage just from colliding with the golem
    // ════════════════════════════════════════════════════════════════

    private void OnCollisionStay(Collision collision)
    {
        TryApplyTouchDamage(collision.collider);
    }

    private void OnTriggerStay(Collider other)
    {
        // Covers the case where the golem's body/player collider is set up as a trigger
        TryApplyTouchDamage(other);
    }

    private void TryApplyTouchDamage(Collider other)
    {
        if (health.isDead) return;
        if (Time.time - lastTouchDamageTime < touchDamageInterval) return;

        bool isPlayer = other.CompareTag("Player")
                     || other.GetComponentInParent<StarterAssets.FirstPersonController>() != null;
        if (!isPlayer) return;

        PlayerHealth ph = playerHealth ?? other.GetComponentInParent<PlayerHealth>();
        if (ph == null || ph.isDead) return;

        ph.TakeDamage(touchDamage);
        lastTouchDamageTime = Time.time;
    }

    // ════════════════════════════════════════════════════════════════
    //  Line of sight
    // ════════════════════════════════════════════════════════════════

    private void UpdateLineOfSight()
    {
        if (player == null)
        {
            playerInSight = false;
            timeSinceLastSeen += Time.deltaTime;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange)
        {
            playerInSight      = false;
            timeSinceLastSeen += Time.deltaTime;
            return;
        }

        // Raycast from Golem eye level toward player chest
        Vector3 eyeOrigin  = transform.position + Vector3.up * 1.8f;
        Vector3 playerMid  = player.position    + Vector3.up * 1.0f;
        Vector3 direction  = (playerMid - eyeOrigin).normalized;

        if (Physics.Raycast(eyeOrigin, direction, out RaycastHit hit, detectionRange))
        {
            bool hitPlayer = hit.collider.CompareTag("Player")
                          || hit.collider.GetComponentInParent<StarterAssets.FirstPersonController>() != null;

            if (hitPlayer)
            {
                playerInSight      = true;
                timeSinceLastSeen  = 0f;
                return;
            }
        }

        playerInSight      = false;
        timeSinceLastSeen += Time.deltaTime;
    }

    /// Returns true if the Golem should currently chase the player.
    private bool ShouldChase()
    {
        return (playerInSight || timeSinceLastSeen < losePlayerTime)
            && player       != null
            && playerHealth != null
            && !playerHealth.isDead;
    }

    // ════════════════════════════════════════════════════════════════
    //  State: IDLE — stands still, plays idle animation, then wanders
    // ════════════════════════════════════════════════════════════════

    private void EnterIdle()
    {
        currentState    = State.Idle;
        agent.isStopped = true;
        stateTimer      = Random.Range(2f, 4f);
        PlayLoop(ANIM_IDLE);
    }

    private void TickIdle()
    {
        if (ShouldChase()) { EnterChase(); return; }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f) EnterWander();
    }

    // ════════════════════════════════════════════════════════════════
    //  State: WANDER — picks a random nearby point and walks to it
    // ════════════════════════════════════════════════════════════════

    private void EnterWander()
    {
        currentState    = State.Wander;
        agent.isStopped = false;
        agent.speed     = wanderSpeed;

        // Pick a random walkable point on the NavMesh within wanderRadius
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
        randomDir.y = 0f;
        Vector3 target = transform.position + randomDir;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Couldn't find a valid point — fall back to idle
            EnterIdle();
            return;
        }

        PlayLoop(ANIM_WALK);
    }

    private void TickWander()
    {
        if (ShouldChase()) { EnterChase(); return; }

        // Arrived at destination → idle again
        if (!agent.pathPending && agent.remainingDistance < 0.6f)
        {
            EnterIdle();
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  State: CHASE — moves toward player at full speed
    // ════════════════════════════════════════════════════════════════

    private void EnterChase()
    {
        currentState    = State.Chase;
        agent.isStopped = false;
        agent.speed     = chaseSpeed;
        PlayLoop(ANIM_WALK);
    }

    private void TickChase()
    {
        if (!ShouldChase()) { EnterIdle(); return; }

        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);

        // No cooldown — as soon as the player is in range, swing.
        if (dist <= attackRange)
        {
            EnterAttack();
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  State: ATTACK — faces player, plays hammer swing once, then
    //  immediately checks if it can swing again (no cooldown).
    // ════════════════════════════════════════════════════════════════

    private void EnterAttack()
    {
        currentState    = State.Attack;
        agent.isStopped = true;
        stateTimer      = attackCycleTime;

        // Face the player on the horizontal plane only
        Vector3 flatTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(flatTarget);

        // Play hammer swing animation (one-shot)
        PlayOneShot(ANIM_ATTACK);

        // Deal damage after animation wind-up delay
        StartCoroutine(DealDamageAfterDelay(attackDamageDelay));
    }

    private void TickAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // No cooldown wait — if the player is still in/near range, swing again right away.
            if (ShouldChase())
            {
                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackRange) EnterAttack();
                else                     EnterChase();
            }
            else
            {
                EnterIdle();
            }
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Event handlers from EnemyHealth
    // ════════════════════════════════════════════════════════════════

    private void HandleTakeDamage()
    {
        if (health.isDead) return;

        // No stun, no hurt animation, no state change — golem keeps doing
        // whatever it was doing (chasing/attacking) and just makes noise.
        PlayHitSound();
    }

    private void HandleDeath()
    {
        StopAllCoroutines();
        currentState = State.Death;

        // Stop movement immediately
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled   = false;
        }

        // Disable all colliders so player doesn't clip into corpse
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Play death animation once — freezes on last frame automatically
        PlayOneShot(ANIM_DEATH);

        // Spawn loot then remove the GameObject after 3 seconds
        DropLoot();
        Destroy(gameObject, 3f);
    }

    // ════════════════════════════════════════════════════════════════
    //  Damage coroutine
    // ════════════════════════════════════════════════════════════════

    private IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (health.isDead)    yield break;
        if (player == null)   yield break;
        if (playerHealth == null || playerHealth.isDead) yield break;

        // Only deal damage if player is still within range
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange + 1.5f)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Audio
    // ════════════════════════════════════════════════════════════════

    private void PlayHitSound()
    {
        if (hitSound == null || audioSource == null) return;
        audioSource.PlayOneShot(hitSound);
    }

    // ════════════════════════════════════════════════════════════════
    //  Loot drop — items drag/skid away from the death point instead
    //  of just popping straight up in place.
    // ════════════════════════════════════════════════════════════════

    private void DropLoot()
    {
        if (dropTable == null || dropTable.Length == 0) return;

        int count = Random.Range(dropMin, dropMax + 1);
        for (int i = 0; i < count; i++)
        {
            ItemData item = dropTable[Random.Range(0, dropTable.Length)];
            if (item == null || item.worldPrefab == null) continue;

            // Random horizontal direction to drag the item away in
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            if (dir2D == Vector2.zero) dir2D = Vector2.right;

            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            GameObject drop = Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

            // Attach WorldLootItem so player can press E to pick it up
            WorldLootItem loot = drop.GetComponent<WorldLootItem>()
                              ?? drop.AddComponent<WorldLootItem>();
            loot.itemData = item;
            loot.quantity = 1;

            // Strong outward + upward impulse, low drag, so it skids away
            // across the ground instead of staying put.
            if (drop.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearDamping = dropLinearDrag;

                Vector3 outward = new Vector3(dir2D.x, 0f, dir2D.y) * dropOutwardForce;
                Vector3 pop     = Vector3.up * dropUpwardForce;
                rb.AddForce(outward + pop, ForceMode.Impulse);

                // A little spin so the drag looks natural, not robotic
                rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  Animation helpers
    // ════════════════════════════════════════════════════════════════

    /// Smooth crossfade into a LOOPING animation (idle, walk).
    private void PlayLoop(string stateName)
    {
        if (anim == null || string.IsNullOrEmpty(stateName)) return;
        anim.CrossFadeInFixedTime(stateName, 0.2f);
    }

    /// Play a ONE-SHOT animation from the very beginning (attack, death).
    private void PlayOneShot(string stateName)
    {
        if (anim == null || string.IsNullOrEmpty(stateName)) return;
        anim.Play(stateName, 0, 0f);
    }
}