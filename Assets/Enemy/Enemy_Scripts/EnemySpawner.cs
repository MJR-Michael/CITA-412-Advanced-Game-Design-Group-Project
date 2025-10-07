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

            GameObject enemyObj = Instantiate(
                info.enemyPrefab,
                info.spawnPoint.position,
                info.spawnPoint.rotation
            );

            // If the enemy implements IChamberBound, set its chamber
            var chamberBound = enemyObj.GetComponent<IChamberBound>();
            chamberBound?.SetChamber(info.chamber);
        }
    }
}
