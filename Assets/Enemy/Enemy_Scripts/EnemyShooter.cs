using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public EnemyAttackConfig attackConfig;
    public Transform firePoint;
    public float rotationSpeed = 10f;

    private float fireTimer;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        Transform target = PlayerTargeting.GetClosestPlayer(transform.position);
        if (target != null)
        {
            // Rotate smoothly toward target
            PlayerTargeting.RotateTowardsTarget(transform, target, rotationSpeed);

            // Fire if cooldown finished
            if (fireTimer <= 0f)
            {
                Fire(target);
                fireTimer = attackConfig.fireRate;
            }
        }
    }

    void Fire(Transform target)
    {
        switch (attackConfig.firePattern)
        {
            case FirePattern.SingleTarget:
                SpawnProjectile(target.position);
                break;

            case FirePattern.Spread:
                SpawnSpread(target.position);
                break;

            case FirePattern.RandomDirection:
                Vector3 randomOffset = Random.insideUnitSphere * 5f;
                SpawnProjectile(target.position + randomOffset);
                break;

            case FirePattern.Area:
                for (int i = 0; i < attackConfig.spreadCount; i++)
                {
                    Vector3 randomArea = target.position + Random.insideUnitSphere * 3f;
                    SpawnProjectile(randomArea);
                }
                break;
        }
    }

    void SpawnProjectile(Vector3 targetPos)
    {
        GameObject proj = Instantiate(attackConfig.projectilePrefab, firePoint.position, Quaternion.identity);
        IProjectile p = proj.GetComponent<IProjectile>();
        if (p != null)
            p.Initialize(targetPos, attackConfig.projectileSpeed);
    }

    void SpawnSpread(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - firePoint.position).normalized;
        int count = Mathf.Max(1, attackConfig.spreadCount);
        float angleStep = attackConfig.spreadAngle / Mathf.Max(1, count - 1);
        float startAngle = -attackConfig.spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleStep;
            Vector3 rotatedDir = Quaternion.Euler(0, angle, 0) * direction;
            SpawnProjectile(firePoint.position + rotatedDir);
        }
    }
}
