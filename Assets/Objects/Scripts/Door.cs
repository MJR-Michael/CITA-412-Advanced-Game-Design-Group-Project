using System;
using UnityEngine;

public class Door : Interactable
{
    public event Action<GridPosition> OnDoorInteractedWith;

    [SerializeField, Tooltip("The door's relative position in the chamber. Reference the chamber design document to get the relative grid position for this chamber.")]
    GridPosition relativeDoorGridPosition;

    [SerializeField, Tooltip("How the door will appear if it does not lead to other chambers")]
    GameObject blockedDoorObj;

    [SerializeField, Tooltip("How the door will appear if it does lead to other chambers")]
    GameObject unblockedDoorObj;

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

    public void Initialize(GridPosition absoluteDoorGridPosition, bool leadsToOtherChamber)
    {
        this.absoluteDoorGridPosition = absoluteDoorGridPosition;
        if (leadsToOtherChamber)
        {
            blockedDoorObj.SetActive(false);
            unblockedDoorObj.SetActive(true);
        }
        else
        {
            blockedDoorObj.SetActive(true);
            unblockedDoorObj.SetActive(false);

            //Prevent the player from interacting with the door
            isInteractable = false;
        }
    }

    public GridPosition RelativeDoorGridPosition() => relativeDoorGridPosition;
    public GridPosition AbsoluteDoorGridPosition() => absoluteDoorGridPosition;
}
