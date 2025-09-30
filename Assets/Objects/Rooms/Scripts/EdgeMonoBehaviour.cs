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

    public Edge GetEdge() => edge;
}
