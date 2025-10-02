using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    public GameObject SpawnEnemyInChamber(ChamberMonoBehaviour chamber)
    {
        if (enemyPrefab == null || chamber == null)
            return null;

        // Get chamber bounds
        Bounds bounds = chamber.GetChamberCollider().bounds;

        // Pick a random spawn point
        Vector3 spawnPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Assign chamber reference to enemy
        if (enemy.TryGetComponent<EnemyTeleport>(out EnemyTeleport teleport))
        {
            teleport.SetCurrentChamber(chamber);
        }

        return enemy;
    }
}
