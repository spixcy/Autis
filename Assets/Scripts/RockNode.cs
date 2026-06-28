using UnityEngine;
using System.Collections;

public class RockNode : MonoBehaviour
{
    [Header("Health")]
    public int hp = 80;

    [Header("Drops")]
    public GameObject[] stoneDropPrefabs;
    public int stoneDropMin = 2;
    public int stoneDropMax = 4;
    
    [Header("Bonus Drops")]
    public GameObject[] smallStoneDropPrefabs;
    [Tooltip("Chance to drop a small stone (0.0 to 1.0)")]
    public float smallStoneChance = 0.4f;

    private bool _isDead = false;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void TakeDamage(int damage, bool isPickaxe)
    {
        if (_isDead) return;

        if (!isPickaxe)
        {
            Debug.Log("You need a pickaxe to mine this rock!");
            return;
        }

        hp -= damage;
        ParticleManager.SpawnRockHit(transform.position + Vector3.up * 0.5f);

        if (hp <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(Wobble());
        }
    }

    private void Die()
    {
        _isDead = true;
        ParticleManager.SpawnRockBreak(transform.position + Vector3.up * 0.5f);

        // Spawn main stone drops
        int dropCount = Random.Range(stoneDropMin, stoneDropMax + 1);
        if (stoneDropPrefabs != null && stoneDropPrefabs.Length > 0)
        {
            for (int i = 0; i < dropCount; i++)
            {
                SpawnDrop(stoneDropPrefabs);
            }
        }

        // Spawn bonus small stone drop
        if (smallStoneDropPrefabs != null && smallStoneDropPrefabs.Length > 0)
        {
            if (Random.value <= smallStoneChance)
            {
                SpawnDrop(smallStoneDropPrefabs);
            }
        }

        gameObject.SetActive(false);
    }

    private void SpawnDrop(GameObject[] prefabs)
    {
        GameObject dropPrefab = prefabs[Random.Range(0, prefabs.Length)];
        Vector3 spawnOffset = Random.insideUnitSphere * 0.5f;
        spawnOffset.y = 0.5f; 

        GameObject drop = Instantiate(dropPrefab, transform.position + spawnOffset, Quaternion.identity);
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 launchForce = Vector3.up * 3f + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            rb.AddForce(launchForce, ForceMode.Impulse);
        }
    }

    private IEnumerator Wobble()
    {
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 wobbleScale = _originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            wobbleScale.y = _originalScale.y + Mathf.Sin(t * Mathf.PI * 2f) * 0.05f;
            wobbleScale.x = _originalScale.x + Mathf.Cos(t * Mathf.PI * 2f) * 0.05f;
            transform.localScale = wobbleScale;
            yield return null;
        }

        transform.localScale = _originalScale;
    }
}
