using UnityEngine;

public class EdgeNodeMonobehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject parentRenderer;

    public void Render()
    {
        parentRenderer.SetActive(true);
    }
    public void Cull()
    {
        parentRenderer.SetActive(false);
    }
}
