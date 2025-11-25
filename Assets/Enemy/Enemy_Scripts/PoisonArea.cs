using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoisonArea : MonoBehaviour
{
    [SerializeField] private float defaultRadius = 3f;
    [SerializeField] private float defaultDuration = 5f;
    [SerializeField] private float defaultDamagePerSecond = 5f;

    private float radius;
    private float duration;
    private float damagePerSecond;
    private HashSet<Player> playersInArea = new HashSet<Player>();

    // Call this to override the prefab defaults
    public void Initialize(float? radiusOverride = null, float? durationOverride = null, float? damageOverride = null)
    {
        radius = radiusOverride ?? defaultRadius;
        duration = durationOverride ?? defaultDuration;
        damagePerSecond = damageOverride ?? defaultDamagePerSecond;

        // Update collider size if needed
        Collider col = GetComponent<Collider>();
        if (col is SphereCollider sphere)
        {
            sphere.radius = radius;
        }

        StartCoroutine(Lifetime());
        StartCoroutine(DamageOverTime());
    }

    private IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            foreach (var player in playersInArea)
            {
                if (player != null)
                    player.TakeDamage(gameObject, damagePerSecond * Time.deltaTime, DamageType.Unknown);
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playersInArea.Add(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playersInArea.Remove(player);
        }
    }
}
