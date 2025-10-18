using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public abstract class Interactable: MonoBehaviour
{
    protected Rigidbody rb;
    protected bool isInteractable = true;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupRigidBody();
    }

    public virtual bool CanBeInteractedWith()
    {
        return isInteractable;
    }

    /// <summary>
    /// Base: Check if the object is interable. If not, logs warning & returns
    /// </summary>
    public virtual void OnInteract()
    {
        if (!isInteractable)
        {
            Debug.LogWarning($"Warning: {gameObject} was interacted with, but it is not set to interactable! Be sure to check if the object is in an interactable");
            return;
        }
    }
    protected abstract void SetupRigidBody();
}
