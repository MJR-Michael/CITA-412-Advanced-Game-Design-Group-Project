using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Optionally, you can add a Rigidbody force instead of moving in Update
        // Rigidbody rb = bolt.GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     rb.linearVelocity = firePoint.forward * 20f;
        // }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"collided with {other.gameObject}");

        // Check if we hit an enemy
        if (other.gameObject.CompareTag("Enemy") && other.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);

        }
        else if (!other.gameObject.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}
