using UnityEngine;

public class DeathKnockback : MonoBehaviour, IDeathBehavior
{
    [SerializeField] private float knockbackForce = 10f;

    public void OnDeath(Enemy enemy)
    {
        //Checks to see if players are nearby
        Collider[] hits = Physics.OverlapSphere(enemy.transform.position, 5f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    //Pushes players
                    Vector3 direction = (hit.transform.position - enemy.transform.position).normalized;
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }
        }
        Debug.Log("Knockback death triggered!");
        Destroy(gameObject, 1f);
    }
}
