using System;
using System.Collections.Generic;
using UnityEngine;

public class ChamberMonoBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject parentRenderer;

    [SerializeField]
    Collider chamberTrigger;

    Chamber chamber;

    bool isRendered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            OnPlayerEnteredChamber();
        }
        //Add other objects that need to handle entering chambers here
    }

    public void OnPlayerEnteredChamber()
    {
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

    public void RenderEdges()
    {
        foreach (EdgeMonoBehaviour edgeMonoBehaviour in chamber.GetEdgeMonoBehaviours())
        {
            if (chamber.OriginGridPosition() == new GridPosition(25, 30))
            {
                Debug.Log("current edge being rendered: " + edgeMonoBehaviour.gameObject.name);
            }
            edgeMonoBehaviour.Render();
        }
    }

    public void CullEdges(EdgeMonoBehaviour edgesNotToCull)
    {
        foreach (EdgeMonoBehaviour edge in chamber.GetEdgeMonoBehaviours())
        {
            if (edgesNotToCull == edge) continue;
            edge.Cull();
        }
    }

    public void Initialize(Chamber chamber)
    {
        //Set chamber data
        this.chamber = chamber;

        //Cull out by default
        parentRenderer.SetActive(false);

        //Set the chamber monobehaviour
        chamber.SetMonobehaviour(this);
    }

    public void CullOutAdjacentChambers(List<Chamber> chambersNotToCullOut)
    {
        //Tell all neighbors to be culled out if they should be
        foreach (Chamber adjacentChamber in chamber.GetConnectingChambers())
        {
            if (chambersNotToCullOut.Contains(adjacentChamber)) { continue; }

            //cull out adjacent chamber's edges, but not the edge current is connecting to

            //tell adjacent neighbors to the adjacent neighbor to cull out their edges
            foreach (Chamber doublyAdjacentNeighbor in adjacentChamber.GetConnectingChambers())
            {
                if (doublyAdjacentNeighbor == chamber) continue; //No looping in on current chamber
                EdgeMonoBehaviour edgeMonoBehaviourBetweenNeighborEdges = adjacentChamber.GetEdgeMonoBehaviourBetweenChambers(
                    doublyAdjacentNeighbor);
                if (edgeMonoBehaviourBetweenNeighborEdges == null) continue;

                doublyAdjacentNeighbor.GetMonobehaviour().CullEdges(edgeMonoBehaviourBetweenNeighborEdges);
            }

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
        RenderEdges();
        isRendered = true;
        foreach (EdgeMonoBehaviour edgeMonoBehaviour in chamber.GetEdgeMonoBehaviours())
        {
            edgeMonoBehaviour.OnChamberRendered();
        }
    }

    public bool IsRendered() => isRendered;
}