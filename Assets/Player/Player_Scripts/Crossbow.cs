using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : MonoBehaviour
{
    [SerializeField]
    List<Collider> playerColliders = new List<Collider>();

    [SerializeField]
    GameObject restingBoltObj;

    public Transform crossbowFirePoint;
    public GameObject crossbowProjectile;

    public float launchSpeed = 60f;
    public float crossbowFireRate = 1f;

    private float crossbowCooldown = 0f;

    bool canFire = true;

    void Update()
    {
        canFire = Time.time >= crossbowCooldown;
        if (canFire)
        {
            restingBoltObj.SetActive(true);
        }
        else
        {
            restingBoltObj.SetActive(false);
        }

        if (canFire && InputManager.Instance.GetPrimaryFireInput() > 0)
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
