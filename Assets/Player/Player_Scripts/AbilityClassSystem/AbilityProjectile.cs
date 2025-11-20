using UnityEngine;

public class ProjectileAbility : AbilityBase
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 30f;

    protected override void ActivateAbility()
    {
        GameObject obj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * projectileSpeed;

        EndAbility();
    }
}
