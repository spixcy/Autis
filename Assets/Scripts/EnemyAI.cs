using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Autis.Inventory;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Patrol, Chase, Attack, Hurt, Death }
    private State currentState;

    [Header("AI Settings")]
    public float detectionRange = 12f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float chaseSpeed = 5f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;

    [Header("Loot Settings")]
    public ItemData[] dropTable;
    public int dropMin = 1;
    public int dropMax = 3;

    [Header("Animation State Names (Exact!)")]
    [SerializeField] private string idleAnim = "idle";
    [SerializeField] private string moveAnim = "walk";
    [SerializeField] private string attackAnim = "attack";
    [SerializeField] private string hurtAnim = "damage";
    [SerializeField] private string deathAnim = "dead";

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyHealth health;
    private Transform player;
    private PlayerHealth playerHealth;

    private float stateTimer;
    private float lastAttackTime;
    private float timeSinceLastSeen;
    private bool isPlayerVisible;

    /*
      EDITOR SETUP FOR NAVMESH:
      1. Window -> AI -> Navigation
      2. Select Terrain -> Navigation Static checkbox ON
      3. Bake tab -> click Bake
      4. Add NavMeshAgent component to each enemy prefab
      5. Attach EnemyAI.cs and EnemyHealth.cs to each enemy prefab
      6. Set Agent radius 0.4, height 1.8, speed 3.5
    */

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<EnemyHealth>();
        
        health.OnDeath += HandleDeath;
        health.OnTakeDamage += HandleTakeDamage;

        var pc = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();
        if (pc != null)
        {
            player = pc.transform;
            playerHealth = pc.GetComponent<PlayerHealth>();
        }

        agent.speed = moveSpeed;
        currentState = State.Idle;
        PlayAnim(idleAnim, true);
    }

    private void Update()
    {
        if (health.isDead) return;

        CheckLineOfSight();

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
            case State.Hurt:
                UpdateHurt();
                break;
        }
    }

    private void HandleTakeDamage()
    {
        if (health.isDead) return;
        SwitchState(State.Hurt);
    }

    private void CheckLineOfSight()
    {
        if (player == null || playerHealth == null || playerHealth.isDead)
        {
            isPlayerVisible = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= detectionRange)
        {
            Vector3 dir = (player.position + Vector3.up * 1.5f) - (transform.position + Vector3.up * 1.5f);
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir.normalized, out RaycastHit hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player") || hit.collider.GetComponentInParent<StarterAssets.FirstPersonController>() != null)
                {
                    isPlayerVisible = true;
                    timeSinceLastSeen = 0f;
                    return;
                }
            }
        }

        isPlayerVisible = false;
        timeSinceLastSeen += Time.deltaTime;
    }

    private void UpdateIdle()
    {
        if (ShouldChase()) { SwitchState(State.Chase); return; }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SwitchState(State.Patrol);
        }
    }

    private void UpdatePatrol()
    {
        if (ShouldChase()) { SwitchState(State.Chase); return; }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SwitchState(State.Idle);
        }
    }

    private void UpdateChase()
    {
        if (!ShouldChase()) { SwitchState(State.Idle); return; }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            SwitchState(State.Attack);
            return;
        }

        agent.SetDestination(player.position);
    }

    private void UpdateAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            lastAttackTime = Time.time;
            SwitchState(State.Chase);
        }
    }

    private void UpdateHurt()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            SwitchState(State.Chase);
        }
    }

    private bool ShouldChase()
    {
        return (isPlayerVisible || timeSinceLastSeen < 3f) && player != null && playerHealth != null && !playerHealth.isDead;
    }

    private void SwitchState(State newState)
    {
        if (health.isDead) return;
        
        currentState = newState;
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                stateTimer = Random.Range(2f, 4f);
                PlayAnim(idleAnim, true);
                break;

            case State.Patrol:
                agent.isStopped = false;
                agent.speed = moveSpeed;
                Vector3 randomDir = Random.insideUnitSphere * 8f;
                randomDir += transform.position;
                if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 8f, 1))
                {
                    agent.SetDestination(hit.position);
                }
                PlayAnim(moveAnim, true);
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = chaseSpeed;
                PlayAnim(moveAnim, true);
                break;

            case State.Attack:
                agent.isStopped = true;
                PlayAnim(attackAnim, false);
                
                Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.LookAt(lookPos);
                
                stateTimer = 1f; // Animation windup + back to idle
                
                StartCoroutine(DealDamageAfterDelay(0.5f));
                break;

            case State.Hurt:
                agent.isStopped = true;
                PlayAnim(hurtAnim, false);
                stateTimer = 0.5f; // Stun duration
                break;
        }
    }

    private IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (health.isDead) yield break;

        if (player != null && playerHealth != null && !playerHealth.isDead)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange + 1.5f) 
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void HandleDeath()
    {
        StopAllCoroutines();
        agent.enabled = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        PlayAnim(deathAnim, false);
        
        DropLoot();

        Destroy(gameObject, 3f);
    }

    private void DropLoot()
    {
        if (dropTable != null && dropTable.Length > 0)
        {
            int numDrops = Random.Range(dropMin, dropMax + 1);
            for (int i = 0; i < numDrops; i++)
            {
                var lootData = dropTable[Random.Range(0, dropTable.Length)];
                if (lootData != null && lootData.worldPrefab != null)
                {
                    Vector2 randOffset = Random.insideUnitCircle * 1f;
                    Vector3 spawnPos = transform.position + Vector3.up * 1f + new Vector3(randOffset.x, 0, randOffset.y);
                    
                    GameObject drop = Instantiate(lootData.worldPrefab, spawnPos, Quaternion.identity);
                    WorldLootItem lootScript = drop.AddComponent<WorldLootItem>();
                    lootScript.itemData = lootData;
                    lootScript.quantity = 1;

                    var oldDrop = drop.GetComponent<DropItem>();
                    if (oldDrop != null) DestroyImmediate(oldDrop);
                    var rb = drop.GetComponent<Rigidbody>();
                    if (rb != null) DestroyImmediate(rb);
                    var colliders = drop.GetComponentsInChildren<Collider>();
                    foreach (var c in colliders) DestroyImmediate(c);
                }
            }
        }
    }

    private void PlayAnim(string animName, bool isLoop)
    {
        if (anim != null && !string.IsNullOrEmpty(animName))
        {
            if (!anim.HasState(0, Animator.StringToHash(animName)))
            {
                // Suppress warning if state doesn't exist, just return
                return;
            }

            if (isLoop)
            {
                anim.CrossFadeInFixedTime(animName, 0.2f);
            }
            else
            {
                anim.Play(animName, 0, 0f);
            }
        }
    }
}
