using UnityEngine;

public class EnemyTeleport : MonoBehaviour
{
    private ChamberMonoBehaviour currentChamber;

    public void SetCurrentChamber(ChamberMonoBehaviour chamber)
    {
        currentChamber = chamber;
    }

    public void Teleport()
    {
        if (currentChamber == null) return;

        // Use the chamber's trigger collider bounds
        Bounds bounds = currentChamber.GetChamberCollider().bounds;

        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );

        transform.position = randomPos;
    }
}
