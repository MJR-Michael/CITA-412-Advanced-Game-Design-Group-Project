using UnityEngine;

public class EnemyTeleport : Enemy
{
    private bool hasTeleportedAtHalfHp = false;

    public override void TakeDamage(float damage)
    {
        // Call the base damage logic (reduces HP, handles death, etc.)
        base.TakeDamage(damage);

        // Check if we just hit half HP for the first time
        if (!hasTeleportedAtHalfHp && currentHealth > 0 && currentHealth <= maxHealth / 2)
        {
            Teleport();
            hasTeleportedAtHalfHp = true;
        }
    }

    public void Teleport()
    {
        if (currentChamber == null)
        {
            Debug.LogWarning("EnemyTeleport: No chamber assigned!");
            return;
        }

        // Disable movement
        var movement = GetComponent<Enemy2_Movement>();
        if (movement != null)
        {
            movement.canMove = false;
        }

        Bounds bounds = currentChamber.GetChamberCollider().bounds;
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        transform.position = randomPos;

        // Update movement start position and re-enable movement
        if (movement != null)
        {
            movement.startPosition = randomPos;
            movement.canMove = true;
        }

        Debug.Log("EnemyTeleported to: " + randomPos);
    }

}