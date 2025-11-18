using System.Collections.Generic;
using UnityEngine;

public class Crossbow : WeaponBase
{
    [Header("Crossbow Stats")]
    [SerializeField] private float crossbowFireRate = 1f;
    [SerializeField] private float launchSpeed = 60f;

    [Header("Crossbow Components")]
    [SerializeField] List<Collider> playerColliders = new List<Collider>();
    [SerializeField] GameObject restingBoltObj;
    [SerializeField] float projectileSpawnPointOffset;


    public Transform playerCameraTransform;
    public GameObject crossbowProjectile;

    void Awake()
    {
        FireRate = crossbowFireRate;
        LaunchSpeed = launchSpeed;
    }

    void Update()
    {
        if (CanFire)
        {
            restingBoltObj.SetActive(true);

            if (InputManager.Instance.GetPrimaryFireInput() > 0)
            {
                Shoot();
            }
        }
        else
        {
            restingBoltObj.SetActive(false);
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + FireRate;

        InvokeShoot();  // Augments react here

        Vector3 projectileOffset = playerCameraTransform.forward * projectileSpawnPointOffset;
        GameObject bolt = Instantiate(
            crossbowProjectile,
            playerCameraTransform.position + projectileOffset,
            playerCameraTransform.rotation
            );

        InvokeProjectileSpawn(bolt); // Augments adjust the projectile if needed

        Rigidbody rb = bolt.GetComponent<Rigidbody>();
        rb.linearVelocity = Camera.main.transform.forward * LaunchSpeed;

        Collider boltCollider = bolt.GetComponent<Collider>();
        if (boltCollider != null)
        {
            foreach (var pc in playerColliders)
                Physics.IgnoreCollision(boltCollider, pc);
        }
    }
}

