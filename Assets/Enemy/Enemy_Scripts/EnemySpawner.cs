using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public void SpawnEnemies(List<EnemySpawnInfo> spawnInfos)
    {
        foreach (var info in spawnInfos)
        {
            if (info.enemyPrefab == null || info.spawnPoint == null)
                continue;

            // TODO: Replace this with pooling for performance
            Instantiate(info.enemyPrefab, info.spawnPoint.position, info.spawnPoint.rotation);
        }
    }
}
