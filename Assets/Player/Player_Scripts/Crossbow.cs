using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : MonoBehaviour
{
    [SerializeField]
    List<Collider> playerColliders = new List<Collider>();

    public Transform crossbowFirePoint;
    public GameObject crossbowProjectile;
    public float crossbowFireRate = 1f;

    private float crossbowCooldown = 0f;

    private void Update()
    {
        crossbowCooldown -= Time.deltaTime;

        if ((InputManager.Instance.GetPrimaryFireInput() > 0) && (crossbowCooldown <= 0f))
        {
            Shoot();
            crossbowCooldown = 1f / crossbowFireRate;
        }

    }

    private void Shoot()
    {
        GameObject bolt = Instantiate(crossbowProjectile, crossbowFirePoint.position, crossbowFirePoint.rotation);
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
