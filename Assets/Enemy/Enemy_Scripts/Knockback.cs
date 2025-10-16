using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackForce = 10f;

    public void ApplyAOEKnockback()
    {
        // Use "transform" to refer to this GameObject
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var controller = hit.GetComponent<StarterAssets.FirstPersonController>();
                if (controller != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    controller.ApplyKnockback(direction * knockbackForce);
                }
            }
        }
    }
}
