using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public static void SpawnWoodHit(Vector3 pos) { /* Implemented in Phase 5 */ }
    public static void SpawnWoodBreak(Vector3 pos) { /* Implemented in Phase 5 */ }
    public static void SpawnRockHit(Vector3 pos) { /* Implemented in Phase 5 */ }
    public static void SpawnRockBreak(Vector3 pos) { /* Implemented in Phase 5 */ }
    public static void SpawnChestOpen(Vector3 pos) { /* Implemented in Phase 5 */ }
    public static void SpawnPickup(Vector3 pos) { /* Implemented in Phase 5 */ }
}
