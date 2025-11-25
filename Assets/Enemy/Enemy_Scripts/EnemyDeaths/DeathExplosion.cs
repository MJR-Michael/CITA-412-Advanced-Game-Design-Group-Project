using UnityEngine;
public class DeathExplosion : MonoBehaviour, IDeathBehavior
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int damage = 20;

    public void OnDeath(Enemy enemy)
    {
        // Spawn explosion 
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Damage nearby objects
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out Player player))
            {
                player.TakeDamage(gameObject, damage, DamageType.Explosion);
            }
        }

        Debug.Log("Explosion death triggered!");
        Destroy(gameObject, 1f);
    }
}
