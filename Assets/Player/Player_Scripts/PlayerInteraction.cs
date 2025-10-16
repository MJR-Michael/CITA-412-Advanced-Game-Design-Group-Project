using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    Canvas interactionOverlayCanvas;

    private void Awake()
    {
        ShowInteractionCanvas(false);
    }

    private void OnTriggerStay(Collider other)
    {
        //Check for interactable object
        if (!other.TryGetComponent<Interactable>(out Interactable interactable)) return;
        if (!interactable.CanBeInteractedWith()) return;

        ShowInteractionCanvas(true);

        if (!InputManager.Instance.PlayerInteract()) return;

        interactable.OnInteract();

        //For now, just hide the canvas after interacting
        ShowInteractionCanvas(false);

        //Handle specific interactable logic in the switch-case below
        switch (interactable)
        {
            case Door door:
                Debug.Log("interacted with door");
                break;
            default:
                Debug.Log("interacted with something that is interactable");
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Interactable>(out Interactable interactable)) return;

        ShowInteractionCanvas(false);
    }

    void ShowInteractionCanvas(bool show)
    {
        if (interactionOverlayCanvas == null)
        {
            Debug.LogWarning($"Warning: {gameObject} does not have a canvas referecne!");
            return;
        }

        interactionOverlayCanvas.enabled = show;
    }
}
