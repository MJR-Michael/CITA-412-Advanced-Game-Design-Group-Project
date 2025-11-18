using UnityEngine;
using System;

public abstract class AbilityBase : MonoBehaviour
{
    [Header("Base Ability Stats")]
    public float Cooldown { get; set; }
    public float Duration { get; set; }

    protected float nextReadyTime = 0f;

    public event Action OnAbilityStart;
    public event Action OnAbilityEnd;
    public event Action OnAbilityTick;
    public event Action<GameObject> OnHit;
    protected bool IsReady => Time.time >= nextReadyTime;
    protected bool IsActive { get; private set; }

    public void TryUseAbility()
    {
        if (!IsReady) return;

        StartAbility();
    }

    protected void StartAbility()
    {
        nextReadyTime = Time.time + Cooldown;
        IsActive = true;

        OnAbilityStart?.Invoke();
        ActivateAbility();
    }

    protected void EndAbility()
    {
        IsActive = false;
        OnAbilityEnd?.Invoke();
    }

    protected void Tick()
    {
        OnAbilityTick?.Invoke();
    }

    // Child ability must implement their specific behavior
    protected abstract void ActivateAbility();  
}
