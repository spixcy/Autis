using UnityEngine;
using System.Collections;

public class TreeHealth : MonoBehaviour
{
    [Header("Health")]
    public int hp = 100;

    [Header("Drops")]
    public GameObject[] woodDropPrefabs;
    public int woodDropMin = 2;
    public int woodDropMax = 5;

    private bool _isDead = false;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        hp -= damage;
        ParticleManager.SpawnWoodHit(transform.position + Vector3.up * 1.5f);

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
        ParticleManager.SpawnWoodBreak(transform.position + Vector3.up * 1f);

        // Spawn drops
        int dropCount = Random.Range(woodDropMin, woodDropMax + 1);
        if (woodDropPrefabs != null && woodDropPrefabs.Length > 0)
        {
            for (int i = 0; i < dropCount; i++)
            {
                GameObject dropPrefab = woodDropPrefabs[Random.Range(0, woodDropPrefabs.Length)];
                Vector3 spawnOffset = Random.insideUnitSphere * 0.8f;
                spawnOffset.y = 1f; // Ensure it spawns slightly above ground

                GameObject drop = Instantiate(dropPrefab, transform.position + spawnOffset, Quaternion.identity);
                Rigidbody rb = drop.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 launchForce = Vector3.up * 3f + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    rb.AddForce(launchForce, ForceMode.Impulse);
                }
            }
        }

        // Hide tree (could spawn stump here)
        gameObject.SetActive(false);
    }

    private IEnumerator Wobble()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 wobbleScale = _originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Simple squish/stretch wobble
            wobbleScale.y = _originalScale.y + Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
            wobbleScale.x = _originalScale.x + Mathf.Cos(t * Mathf.PI * 2f) * 0.1f;
            transform.localScale = wobbleScale;
            yield return null;
        }

        transform.localScale = _originalScale;
    }
}
