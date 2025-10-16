using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyProximityExplode : MonoBehaviour
{
    [SerializeField] private float triggerRadius = 1.5f;
    private SphereCollider triggerCollider;
    private Enemy enemy;
    private bool isTriggered = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.radius = triggerRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.TryGetComponent(out Player player))
        {
            isTriggered = true;
            Debug.Log($"{enemy.name} triggered proximity death!");

            enemy.Die();
        }
    }
}
