using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0)
        {
            Debug.Log("Player is dead and cannot be healed!");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"Player healed {amount}. Current HP: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        Debug.Log("Player died!");
    }
}
