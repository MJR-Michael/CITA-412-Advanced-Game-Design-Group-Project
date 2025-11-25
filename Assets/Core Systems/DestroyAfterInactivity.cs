using System;
using System.Collections;
using UnityEngine;

public class DestroyAfterInactivity : MonoBehaviour
{
    [SerializeField]
    float inactivityDurationBeforeDestruction;

    public Action OnActivity;
    Coroutine inactivityCoroutine;

    private void Awake()
    {
        OnActivity += ResetInactivity;
    }

    private void ResetInactivity()
    {
        if (inactivityCoroutine != null)
        {
            StopCoroutine(inactivityCoroutine);
        }

        inactivityCoroutine = StartCoroutine(BeginInactivityDuration());
    }

    public void InvokeOnActivity()
    {
        OnActivity?.Invoke();
    }

    IEnumerator BeginInactivityDuration()
    {
        yield return new WaitForSeconds(inactivityDurationBeforeDestruction);
        Destroy(gameObject);
    }
}
