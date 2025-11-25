using UnityEngine;

public class LaserProjectile : MonoBehaviour, IProjectile
{
    public float speed = 10f;
    public float damage = 10f;
    private Vector3 direction;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    // Implement IProjectile
    public void Initialize(Vector3 targetPosition, float projectileSpeed)
    {
        direction = (targetPosition - transform.position).normalized;
        speed = projectileSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            player.TakeDamage(gameObject, damage, DamageType.Projectile);
            Destroy(gameObject);
        }
    }
}
