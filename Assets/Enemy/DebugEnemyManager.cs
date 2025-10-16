using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugEnemyManager : MonoBehaviour
{
    [SerializeField] private ChamberMonoBehaviour chamber; // Assign the chamber in inspector
    [SerializeField] private Key f1Key = Key.F1; // Key to spawn enemies
    [SerializeField] private Key f2Key = Key.F2; // Key to assign all enemies to chamber

    private bool hasSpawned = false;

    private void Update()
    {
        // Spawn enemies in chamber
        if (Keyboard.current[f1Key].wasPressedThisFrame && !hasSpawned)
        {
            SpawnEnemies();
            hasSpawned = true;
        }

        // Assign all enemies in scene to this chamber
        if (Keyboard.current[f2Key].wasPressedThisFrame)
        {
            AssignAllEnemiesToChamber();
        }
    }

    private void SpawnEnemies()
    {
        if (chamber == null)
        {
            Debug.LogError("No chamber assigned to DebugEnemySpawner!");
            return;
        }

        List<EnemySpawnInfo> spawnInfos = chamber.GetSpawnInfos();

        if (spawnInfos.Count == 0)
        {
            Debug.LogWarning("No enemies to spawn in this chamber.");
            return;
        }

        foreach (var info in spawnInfos)
        {
            if (info.enemyPrefab != null && info.spawnPoint != null)
            {
                GameObject enemyObj = Instantiate(info.enemyPrefab, info.spawnPoint.position, info.spawnPoint.rotation);

                // Assign the spawned enemy to this chamber if it has EnemyTeleport script
                EnemyTeleport enemyTeleport = enemyObj.GetComponent<EnemyTeleport>();
                if (enemyTeleport != null)
                {
                    enemyTeleport.currentChamber = chamber;
                }

                Debug.Log($"Spawned {info.enemyPrefab.name} at {info.spawnPoint.position}");
            }
            else
            {
                Debug.LogWarning("Enemy prefab or spawn point is missing in EnemySpawnInfo.");
            }
        }
    }

    private void AssignAllEnemiesToChamber()
    {
        if (chamber == null)
        {
            Debug.LogError("No chamber assigned for enemy assignment!");
            return;
        }

        EnemyTeleport[] allEnemies = FindObjectsByType<EnemyTeleport>(FindObjectsSortMode.None);
        if (allEnemies.Length == 0)
        {
            Debug.LogWarning("No EnemyTeleport instances found in the scene.");
            return;
        }

        foreach (EnemyTeleport enemy in allEnemies)
        {
            enemy.currentChamber = chamber;
            Debug.Log($"Assigned {enemy.name} to chamber {chamber.name}");
        }

        Debug.Log($"Assigned {allEnemies.Length} enemies to chamber {chamber.name}");
    }
}
