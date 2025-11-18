using UnityEngine;
using System.Collections;

public class AuraAbility : AbilityBase
{
    [SerializeField] float damagePerSecond;
    [SerializeField] float radius;

    protected override void ActivateAbility()
    {
        StartCoroutine(DoAura());
    }

    IEnumerator DoAura()
    {
        float timer = 0f;

        while (timer < Duration)
        {
            Tick();
            DamageNearbyEnemies();
            timer += Time.deltaTime;
            yield return null;
        }

        EndAbility();
    }

    void DamageNearbyEnemies()
    {
        // Example: overlap sphere + deal damage
    }
}
