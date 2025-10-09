using UnityEngine;

public class EnemyTeleport : Enemy
{
    public void Teleport()
    {
        if (currentChamber == null) return;

        Bounds bounds = currentChamber.GetChamberCollider().bounds;
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        transform.position = randomPos;
    }
}
