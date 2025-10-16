using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;

    // Keep a list of registered chambers
    private List<ChamberMonoBehaviour> registeredChambers = new List<ChamberMonoBehaviour>();

    /// Called by a chamber to register itself.
    public void RegisterChamber(ChamberMonoBehaviour chamber)
    {
        if (!registeredChambers.Contains(chamber))
        {
            registeredChambers.Add(chamber);
            chamber.OnPlayerEntered += HandleChamberEntered;
        }
    }

    private void HandleChamberEntered(ChamberMonoBehaviour chamber)
    {
        if (spawner == null) return;
        spawner.SpawnEnemies(chamber.GetSpawnInfos());
    }
}
