public class DeathKnockback : MonoBehaviour, IDeathBehavior
{
    [SerializeField] private float knockbackForce = 10f;

    public void OnDeath(Enemy enemy)
    {
        Collider[] hits = Physics.OverlapSphere(enemy.transform.position, 5f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var controller = hit.GetComponent<StarterAssets.FirstPersonController>();
                if (controller != null)
                {
                    Vector3 direction = (hit.transform.position - enemy.transform.position).normalized;
                    controller.ApplyKnockback(direction * knockbackForce);
                }
            }
        }

        Debug.Log("Knockback death triggered!");
        Destroy(gameObject, 1f);
    }
}
