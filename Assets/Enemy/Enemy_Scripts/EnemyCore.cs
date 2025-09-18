using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    private IDeathBehavior[] deathBehaviors;

    private void Awake()
    {
        deathBehaviors = GetComponents<IDeathBehavior>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
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
}
