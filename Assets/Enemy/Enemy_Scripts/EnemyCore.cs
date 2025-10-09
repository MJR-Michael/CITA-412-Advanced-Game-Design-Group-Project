using UnityEngine;

public class Enemy : MonoBehaviour, IChamberBound
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    private IDeathBehavior[] deathBehaviors;
    protected ChamberMonoBehaviour currentChamber;

    private void Awake()
    {
        deathBehaviors = GetComponents<IDeathBehavior>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is dead and cannot be healed!");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"Enemy healed {amount}. Current HP: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        foreach (var behavior in deathBehaviors)
        {
            behavior.OnDeath(this);
        }
    }

    public void SetChamber(ChamberMonoBehaviour chamber)
    {
        currentChamber = chamber;
    }

    public ChamberMonoBehaviour GetChamber() => currentChamber;
}
