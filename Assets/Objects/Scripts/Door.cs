using System;
using UnityEngine;

public class Door : Interactable
{
    public event Action<GridPosition> OnDoorInteractedWith;

    [SerializeField, Tooltip("The door's relative position in the chamber. Reference the chamber design document to get the relative grid position for this chamber.")]
    GridPosition relativeDoorGridPosition;

    GridPosition absoluteDoorGridPosition;

    public void SetDoorwayInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }

    public override void OnInteract()
    {
        base.OnInteract();
        OpenDoor();
        OnDoorInteractedWith?.Invoke(absoluteDoorGridPosition);
    }

    protected override void SetupRigidBody()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void OpenDoor()
    {
        //For now, just hide the door
        gameObject.SetActive(false);
    }

    public void Initialize(GridPosition chamberOriginGridPosition)
    {
        absoluteDoorGridPosition = chamberOriginGridPosition + relativeDoorGridPosition;
    }

    public GridPosition AbsoluteDoorGridPosition() => absoluteDoorGridPosition;
}
