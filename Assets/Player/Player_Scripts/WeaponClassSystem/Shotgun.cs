using System;
using Unity.Mathematics;
using UnityEngine;

public class Shotgun : WeaponBase
{
    public Action OnShotgunSpreadAngleDegreesChanged;

    [SerializeField]
    float maxShootDistance = 30f;

    [SerializeField]
    int pelletCount = 8;

    [SerializeField]
    float shotgunSpreadAngleDegrees;

    [SerializeField]
    int pelletDamage = 10;

    [SerializeField]
    Transform startingShootPos;

    [SerializeField]
    Transform shotgunShootDir;

    float shotgunSpreadAngleRad;
    float shotgunSpreadRadius;

    public Transform GetStartingShootPos()
    {
        return startingShootPos;
    }
    public Transform GetShotgunShootDir()
    {
        return shotgunShootDir;
    }

    private void Awake()
    {
        OnShoot += ShootShotgun;
        shotgunSpreadAngleRad = shotgunSpreadAngleDegrees * Mathf.Deg2Rad;
        shotgunSpreadRadius = Mathf.Tan(shotgunSpreadAngleRad);
    }


    private void Update()
    {
        if (!CanFire) return;

        if (InputManager.Instance.GetPrimaryFireInputPressedThisFrame())
        {
            //Shoot the shotgun
            InvokeShoot();
        }
    }

    public float GetShotgunSpreadDegrees()
    {
        return shotgunSpreadAngleDegrees;
    }
    public float GetShotgunSpreadRadius()
    {
        return shotgunSpreadRadius;
    }

    public void ShootShotgun()
    {
        nextFireTime = Time.time + FireRate;

        Debug.Log("Shot");

        for (int i = 0; i < pelletCount; i++)
        {
            //Set a random shoot direction
            SetRandomShootDir();

            Ray ray = new Ray(startingShootPos.position, shotgunShootDir.position - startingShootPos.position);
            Debug.DrawRay(startingShootPos.position, shotgunShootDir.position-startingShootPos.position, Color.red, 1f);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxShootDistance)) continue;


            Debug.Log("hit something");

            //Pellet hit something
            InvokeHit();

            //Handle hitting different things
            if (hit.collider.transform.TryGetComponent<Enemy>(out Enemy enemenemy))
            {
                enemenemy.TakeDamage(pelletDamage);
            }
            else if (hit.collider.transform.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(gameObject, pelletDamage, DamageType.Projectile);
            }
        }
    }

    void SetRandomShootDir()
    {
        //1) Find the x-component of the shoot direction. x-value must be between 0 and shoot radius
        //2) Based on the x-component, calculate the y-component. Remember that r^2=x^2+y^2 -> y_max=sqrt(r^2-x^2).
        //3) Get the random y-component between 0 and y_max

        float x = UnityEngine.Random.Range(0, shotgunSpreadRadius);
        float yMax = Mathf.Sqrt((shotgunSpreadRadius * shotgunSpreadRadius)-(x*x));
        float y = UnityEngine.Random.Range(0, yMax);

        //Set the shoot direction's x and y-components
        shotgunShootDir.localPosition = new Vector3(
            x,
            y,
            shotgunShootDir.localPosition.z
            );
    }
}
