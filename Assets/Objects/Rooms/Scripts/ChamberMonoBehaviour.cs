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
    public event Action<ChamberMonoBehaviour> OnPlayerEntered;

    [Header("Rendering & Triggers")]
    [SerializeField] 
    private GameObject parentRenderer;
    
    [SerializeField] 
    private Collider chamberTrigger;


    [Header("Enemy Spawning")]
    [SerializeField] 
    private List<EnemySpawnInfo> enemySpawnInfos = new(); // Per-prefab spawn data


    [Header("References")]
    [SerializeField]
    GameObject mapRenderer;

    //NOTE: change item drop position in the future. For now, this will suffice
    [SerializeField, Tooltip("Where will the item reward spawn in this chamber?")]
    GameObject itemRewardSpawnPoint;

    [SerializeField, Tooltip("The doors in the chamber that lead to other chambers")]
    Door[] doorsInChamber;

    private Chamber chamber;
    private bool isRendered;
    bool hasBeenVisisted;

    private void Start()
    {
        //Setup door references
        foreach (Door door in doorsInChamber)
        {
            GridPosition absoluteDoorGridPosition = door.RelativeDoorGridPosition() + chamber.OriginGridPosition();

            //Check determine if this door is one that leads to another chamber
            bool leadsToOtherChamber = chamber.IsUsingHallwayConnector(absoluteDoorGridPosition);

            door.Initialize(absoluteDoorGridPosition, leadsToOtherChamber);

            //Listen to when the door is interacted with
            if (leadsToOtherChamber)
            {
                door.OnDoorInteractedWith += HandleDoorInteractedWith;
            }
        }
    }

    private void HandleDoorInteractedWith(GridPosition absoluteDoorGridPosition)
    {
        //Tell the door on the other side of the hallway to open
        Door otherChamberDoor = chamber.GetOtherChamberDoorFromEdge(absoluteDoorGridPosition);
        if (otherChamberDoor != null)
        {
            otherChamberDoor.OpenDoor();
        }
    }

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

        // SPAWN ENEMIES ONCE WHEN PLAYER ENTERS
        foreach (var spawnInfo in enemySpawnInfos)
        {
            if (spawnInfo.enemyPrefab != null && spawnInfo.spawnPoint != null)
            {
                Instantiate(spawnInfo.enemyPrefab, spawnInfo.spawnPoint.position, spawnInfo.spawnPoint.rotation);
            }
        }

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

        mapRenderer.SetActive(false);

        //Set the chamber monobehaviour
        chamber.SetMonobehaviour(this);

        //Setup chamber based on the type it is
        switch (chamber.GetChamberType())
        {
            case Chamber.ChamberType.ItemReward:
                //Instantiate the item reward (if it exists) into the chamber
                if (chamber.GetItemReward() != null)
                {
                    GameObject itemRewardObj = Instantiate(chamber.GetItemReward().gameObject, parentRenderer.transform);
                    itemRewardObj.transform.position = itemRewardSpawnPoint.transform.position;
                }
                break;
            default:
                //Debug.Log($"Chamber type {chamber.GetChamberType()} not customized yet");
                break;
        }

        Cull();
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

    public Door GetDoorAtPosition(GridPosition edgeConnectorGridPosition)
    {
        //Find the door at the given grid position
        foreach (Door door in doorsInChamber)
        {
            if (door.AbsoluteDoorGridPosition() == edgeConnectorGridPosition)
            {
                return door;
            }
        }

        return null;
    }
}
