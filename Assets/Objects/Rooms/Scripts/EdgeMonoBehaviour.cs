using System;
using UnityEngine;

public class EdgeMonoBehaviour : MonoBehaviour
{
    Edge edge;
    GameObject parentRenderer;


    public void Initialize(Edge edge, GameObject parentRenderer)
    {
        this.edge = edge;
        this.parentRenderer = parentRenderer;
        edge.SetEdgeMonobehaviour(this);

        parentRenderer.SetActive(false);
    }

    public void Render()
    {
        parentRenderer.SetActive(true);
    }
    public void Cull()
    {
        parentRenderer.SetActive(false);
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