using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public event System.Action OnShoot;
    public event System.Action<GameObject> OnProjectileSpawned;  
    public event System.Action OnHit;

    public float FireRate { get; set; }
    public float LaunchSpeed { get; set; }

    protected float nextFireTime = 0f;

    protected bool CanFire => Time.time >= nextFireTime;

    protected void InvokeShoot()
    {
        OnShoot?.Invoke();
    }

    protected void InvokeProjectileSpawn(GameObject proj)
    {
        OnProjectileSpawned?.Invoke(proj);
    }

    protected void InvokeHit()
    {
        OnHit?.Invoke();
    }
}
