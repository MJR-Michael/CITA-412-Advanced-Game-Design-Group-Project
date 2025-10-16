using UnityEngine;

public class AlternateCrossbow : MonoBehaviour
{
    public Transform crossbowFirePoint;
    public GameObject AlternatecrossbowProjectilePrefab;

    public float launchSpeed = 60f;
    public float crossbowFireRate = 1f;

    private float crossbowCooldown = 0f;

    void Update()
    {
        if (InputManager.Instance.GetPrimaryFireInput() > 0 && Time.time >= crossbowCooldown)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        crossbowCooldown = Time.time + crossbowFireRate;

        // Combine spawn rotation with prefab rotation
        Quaternion spawnRotation = crossbowFirePoint.rotation * AlternatecrossbowProjectilePrefab.transform.rotation;
        GameObject boltGO = Instantiate(AlternatecrossbowProjectilePrefab, crossbowFirePoint.position, spawnRotation);

        // Get the projectile script, not the Rigidbody
        AlternateCrossbowProjectile boltProj = boltGO.GetComponent<AlternateCrossbowProjectile>();
        if (boltProj != null)
        {
            // Set velocity for manual movement
            boltProj.velocity = crossbowFirePoint.forward * launchSpeed;
        }
    }
}
