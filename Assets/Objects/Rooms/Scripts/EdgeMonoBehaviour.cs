using System;
using UnityEngine;

public class EdgeMonoBehaviour : MonoBehaviour
{
    Edge edge;

    EdgeNodeMonobehaviour[] edgeNodes;

    public void Initialize(Edge edge)
    {
        this.edge = edge;
        edge.SetEdgeMonobehaviour(this);
        edgeNodes = GetComponentsInChildren<EdgeNodeMonobehaviour>();

        Cull();
    }

    public void Render()
    {
        foreach (EdgeNodeMonobehaviour edge in edgeNodes)
        {
            edge.Cull();
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