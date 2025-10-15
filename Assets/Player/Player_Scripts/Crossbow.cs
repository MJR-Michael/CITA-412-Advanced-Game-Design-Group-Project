using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : MonoBehaviour
{
    [SerializeField]
    List<Collider> playerColliders = new List<Collider>();

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
        Debug.Log("hi");
        crossbowCooldown = Time.time + crossbowFireRate;

        Quaternion spawnRotation = crossbowFirePoint.rotation * crossbowProjectile.transform.rotation;
        GameObject bolt = Instantiate(crossbowProjectile, crossbowFirePoint.position, spawnRotation);

        Rigidbody rb = bolt.GetComponent<Rigidbody>();
        rb.linearVelocity = Camera.main.transform.forward * launchSpeed;

        Collider boltCollider = bolt.GetComponent<Collider>();

        if (boltCollider != null)
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(boltCollider, playerCollider);
            }
        }
    }
}
