
using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    public static List<Player> AllPlayers = new List<Player>();

    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    public bool isInvulnerable = false;

    private void OnEnable()
    {
        AllPlayers.Add(this);
    }

    private void OnDisable()
    {
        AllPlayers.Remove(this);
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable)
        {
            return;
        }

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
