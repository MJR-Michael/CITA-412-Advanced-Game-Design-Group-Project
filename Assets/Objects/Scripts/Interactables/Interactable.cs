using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Interactable: MonoBehaviour
{
    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupRigidBody();
    }

    public abstract bool CanBeInteractedWith();
    public abstract void OnInteract();
    protected abstract void SetupRigidBody();
}
