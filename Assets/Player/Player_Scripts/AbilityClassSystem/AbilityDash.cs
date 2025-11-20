using UnityEngine;
using System.Collections;

public class AbilityDash : AbilityBase
{
    [SerializeField] float dashSpeed = 20f;

    protected override void ActivateAbility()
    {
        // Movement logic
        StartCoroutine(DoDash());
    }

    private IEnumerator DoDash()
    {
        float timer = 0f;

        while (timer < Duration)
        {
            transform.position += transform.forward * dashSpeed * Time.deltaTime;
            Tick();
            timer += Time.deltaTime;
            yield return null;
        }

        EndAbility();
    }
}
