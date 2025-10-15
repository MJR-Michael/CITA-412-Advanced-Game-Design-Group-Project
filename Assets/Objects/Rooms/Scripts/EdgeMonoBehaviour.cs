using System;
using UnityEngine;

public class EdgeMonoBehaviour : MonoBehaviour
{
    Edge edge;
    GameObject parentRenderer;

    EdgeNodeMonobehaviour[] edgeNodes;

    public void Initialize(Edge edge, GameObject parentRenderer)
    {
        this.edge = edge;
        edge.SetEdgeMonobehaviour(this);
        this.parentRenderer = parentRenderer;
        edgeNodes = parentRenderer.GetComponentsInChildren<EdgeNodeMonobehaviour>();

        foreach (EdgeNodeMonobehaviour node in edgeNodes)
        {
            node.Initialize();
        }

        Cull();
    }

    public void Render()
    {
        //Debug.Log("rendering");
        foreach (EdgeNodeMonobehaviour edge in edgeNodes)
        {
            //Debug.Log(edge.gameObject.name);

            edge.Render();
        }
    }
    public void Cull()
    {
        foreach (EdgeNodeMonobehaviour edge in edgeNodes)
        {
            edge.Cull();
        }
    }

    public void OnChamberRendered()
    {
        UpdateEdgeRender();
        CheckIfShouldDisplayToMap();
    }
    private void CheckIfShouldDisplayToMap()
    {
        if (edge.GetChamberA().GetMonobehaviour().HasBeenVisisted() &&
            edge.GetChamberB().GetMonobehaviour().HasBeenVisisted())
        {
            //Show up on the map
            foreach (EdgeNodeMonobehaviour edge in edgeNodes)
            {
                edge.RenderOnMap();
            }
        }
    }

    private void UpdateEdgeRender()
    {
        if (edge.GetChamberA().GetMonobehaviour().IsRendered() ||
            edge.GetChamberB().GetMonobehaviour().IsRendered())
        {
            Render();
        }
        else
        {
            Cull();
        }
    }

    public Edge GetEdge() => edge;

    public void OnChamberCulled()
    {
        UpdateEdgeRender();
    }
}