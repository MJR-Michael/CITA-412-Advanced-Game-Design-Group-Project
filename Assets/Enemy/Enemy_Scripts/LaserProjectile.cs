using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move the laser forward
        transform.position += direction * speed * Time.deltaTime;
    }

    // Called by the spawner to set the target
    public void Initialize(Vector3 targetPosition)
    {
        // Calculate direction toward target
        direction = (targetPosition - transform.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Example: damage players
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit player!");
            // TODO: Call PlayerCore.TakeDamage() here
            Destroy(gameObject);
        }
    }
}
