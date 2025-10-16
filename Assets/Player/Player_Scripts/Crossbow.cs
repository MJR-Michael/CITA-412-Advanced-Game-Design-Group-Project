using UnityEngine;

public class Crossbow : MonoBehaviour
{
    public Transform crossbowFirePoint;
    public GameObject crossbowProjectile;

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

        Quaternion spawnRotation = crossbowFirePoint.rotation * crossbowProjectile.transform.rotation;
        GameObject bolt = Instantiate(crossbowProjectile, crossbowFirePoint.position, spawnRotation);

        Rigidbody rb = bolt.GetComponent<Rigidbody>();
        rb.linearVelocity = Camera.main.transform.forward * launchSpeed;
    }
}
