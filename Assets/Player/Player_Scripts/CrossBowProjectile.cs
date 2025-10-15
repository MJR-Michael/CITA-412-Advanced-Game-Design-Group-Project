using UnityEngine;

public class CrossbowProjectile : MonoBehaviour
{
    Collider collider;
    private Rigidbody rb;
    public float lifeTime = 8f;       // Destroy after x seconds if it doesn’t hit anything
    public int damage = 10;           // Damage dealt on hit
    private bool stuck = false;

    void Awake()
    {
        collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;
        stuck = true;

        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }

        // Stop physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point;

        // Stick along the flight direction
        Vector3 stickDirection = rb.linearVelocity.normalized;
        if (stickDirection == Vector3.zero) stickDirection = transform.forward;

        // Make the arrow look along the flight direction while aligning with surface normal
        transform.rotation = Quaternion.LookRotation(stickDirection, contact.normal);
        if (collision.rigidbody != null)
        {
            transform.SetParent(collision.rigidbody.transform);
        }
        else
        {
            transform.SetParent(collision.transform);
        }
        Destroy(rb);
        Destroy(collider);
    }
}