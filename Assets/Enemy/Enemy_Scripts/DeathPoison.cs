using UnityEngine;

public class DeathPoison : MonoBehaviour, IDeathBehavior
{
    [SerializeField] private GameObject poisonPrefab;
    [SerializeField] private float radius = 3f;          // per-enemy override
    [SerializeField] private float duration = 5f;        // per-enemy override
    [SerializeField] private float damagePerSecond = 5f; // per-enemy override

    public void OnDeath(Enemy enemy)
    {
        if (poisonPrefab != null)
        {
            GameObject poison = Instantiate(poisonPrefab, enemy.transform.position, Quaternion.identity);

            PoisonArea poisonArea = poison.GetComponent<PoisonArea>();
            if (poisonArea != null)
            {
                // Apply per-enemy overrides
                poisonArea.Initialize(radius, duration, damagePerSecond);
            }
        }

        Debug.Log($"{enemy.name} spawned poison on death!");
        Destroy(gameObject, 1f);
    }
}
