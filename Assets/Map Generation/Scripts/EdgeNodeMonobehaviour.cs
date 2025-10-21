using System;
using UnityEngine;

public class EdgeNodeMonobehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject parentRenderer;

    [SerializeField]
    GameObject mapRenderer;

    public void Initialize()
    {
        mapRenderer.SetActive(false);
        Cull();
    }

    public void Render()
    {
        parentRenderer.SetActive(true);
    }
    public void Cull()
    {
        parentRenderer.SetActive(false);
    }

    public void RenderOnMap()
    {
        mapRenderer.SetActive(true);
    }
}
