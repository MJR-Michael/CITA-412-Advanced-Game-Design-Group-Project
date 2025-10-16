using System;
using UnityEngine;

public class Door : Interactable
{

    public override bool CanBeInteractedWith()
    {
        //For now, just make the door always interactable
        return true;
    }

    public override void OnInteract()
    {
        OpenDoor();
    }

    protected override void SetupRigidBody()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OpenDoor()
    {
        //For now, just hide the door
        gameObject.SetActive(false);
    }
}
