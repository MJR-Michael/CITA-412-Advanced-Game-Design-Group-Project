using UnityEngine;

public class AlternateCrossbowProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifeTime = 8f;
    public int damage = 10;
    public float speed = 25f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask;

    public Vector3 velocity;
    private bool stuck = false;

    void Start()
    {
        // Kinematic Rigidbody to prevent physics bouncing
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // optional safety

        // Set initial velocity along forward direction
        velocity = transform.forward * speed;

        // Destroy projectile after lifetime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (stuck) return;

        Vector3 movement = velocity * Time.deltaTime;

        // Raycast along movement to detect collision
        if (Physics.Raycast(transform.position, movement.normalized, out RaycastHit hit, movement.magnitude, collisionMask))
        {
            HandleHit(hit.collider, hit.point, hit.normal);
            return;
        }

        // Move manually
        transform.position += movement;
    }

    private void HandleHit(Collider collider, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (stuck) return;
        stuck = true;

        // Apply damage if the collider has an Enemy component
        if (collider.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damage);
        }

        // Stop movement
        velocity = Vector3.zero;

        // Snap to hit point
        transform.position = hitPoint;

        // Align along flight direction while respecting surface normal
        Vector3 stickDirection = transform.forward;
        transform.rotation = Quaternion.LookRotation(stickDirection, -hitNormal);

        // Parent to target so it moves with it
        transform.SetParent(collider.transform);
    }
}
