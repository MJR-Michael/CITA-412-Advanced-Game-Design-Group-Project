using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    [HideInInspector] public ChamberMonoBehaviour chamber;
}


public class ChamberMonoBehaviour : MonoBehaviour
{
    [Header("Rendering & Triggers")]
    [SerializeField] private GameObject parentRenderer;
    [SerializeField] private Collider chamberTrigger;

    [Header("Enemy Spawning")]
    [SerializeField] private List<EnemySpawnInfo> enemySpawnInfos = new(); // Per-prefab spawn data

    [SerializeField]
    GameObject mapRenderer;

    private Chamber chamber;
    private bool isRendered;
    public event Action<ChamberMonoBehaviour> OnPlayerEntered;
    bool hasBeenVisisted;


    // TRIGGER HANDLING
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            HandlePlayerEnteredChamber();
            OnPlayerEntered?.Invoke(this); // Notify the manager or spawner system
        }
    }

    public void HandlePlayerEnteredChamber()
    {
        hasBeenVisisted = true;
        mapRenderer.SetActive(true);
        Render();

        //Tell adjacent neighbors to cull out their neighbors, excluding this chamber & the ones this chamber connects to.
        List<Chamber> chambersNotToCullOut = new List<Chamber>(chamber.GetConnectingChambers());
        chambersNotToCullOut.Add(chamber);

        //Tell adjacent neighbors to render and to cull out their neighbors
        foreach (Chamber adjacentChamber in chamber.GetConnectingChambers())
        {
            adjacentChamber.GetMonobehaviour().Render();
            adjacentChamber.GetMonobehaviour().CullOutAdjacentChambers(chambersNotToCullOut);
        }
    }

    public void Initialize(Chamber chamber)
    {
        //Set chamber data
        this.chamber = chamber;

        Cull();

        mapRenderer.SetActive(false);

        //Set the chamber monobehaviour
        chamber.SetMonobehaviour(this);
    }

    public void CullOutAdjacentChambers(List<Chamber> chambersNotToCullOut)
    {
        //Tell all neighbors to be culled out if they should be
        foreach (Chamber adjacentChamber in chamber.GetConnectingChambers())
        {
            if (chambersNotToCullOut.Contains(adjacentChamber)) { continue; }
            //cull out the object
            adjacentChamber.GetMonobehaviour().Cull();
        }
    }

    public void Cull()
    {
        parentRenderer.SetActive(false);
        chamberTrigger.enabled = false;
        isRendered = false;

        foreach (EdgeMonoBehaviour edgeMonoBehaviour in chamber.GetEdgeMonoBehaviours())
        {
            edgeMonoBehaviour.OnChamberCulled();
        }
    }

    public void Render()
    {
        parentRenderer.SetActive(true);
        chamberTrigger.enabled = true;
        isRendered = true;

        foreach (EdgeMonoBehaviour edgeMonoBehaviour in chamber.GetEdgeMonoBehaviours())
        {
            edgeMonoBehaviour.OnChamberRendered();
        }
    }

    // ACCESSORS
    public List<EnemySpawnInfo> GetSpawnInfos()
    {
        // Assign this chamber reference to each spawn info
        foreach (var info in enemySpawnInfos)
        {
            info.chamber = this;
        }

        return enemySpawnInfos;
    }
    public Collider GetChamberCollider() => chamberTrigger;
    public bool IsRendered() => isRendered;
    public bool HasBeenVisisted() => hasBeenVisisted;
}