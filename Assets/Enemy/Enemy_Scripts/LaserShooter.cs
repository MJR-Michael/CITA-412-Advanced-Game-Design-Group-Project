using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform firePoint; // empty child object for spawn position
    public float fireCooldown = 2f;

    private float fireTimer;

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            ShootAtClosestPlayer();
            fireTimer = fireCooldown;
        }
    }

    private void ShootAtClosestPlayer()
    {
        Transform target = PlayerTargeting.GetClosestPlayer(transform.position);
        if (target == null) return;

        // Spawn laser
        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);

        // Tell the laser where to go
        laser.GetComponent<LaserProjectile>().Initialize(target.position);
    }
}
