using UnityEngine;
using System.Collections;

public class DeathPhaseChange : MonoBehaviour, IDeathBehavior
{
    [SerializeField] private GameObject phase2EnemyPrefab;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float waitAfterGrounded = 1f;
    [SerializeField] private float maxFallTime = 20f;

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void OnDeath(Enemy enemy)
    {
        rb.useGravity = true; // start falling
        StartCoroutine(HandleFallAndRespawn());
    }

    private IEnumerator HandleFallAndRespawn()
    {
        float elapsedTime = 0f;

        // Wait until grounded or timeout
        while (!IsGrounded() && elapsedTime < maxFallTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // CASE 1: grounded -> wait, then spawn phase 2
        if (IsGrounded())
        {
            yield return new WaitForSeconds(waitAfterGrounded);

            if (phase2EnemyPrefab != null)
            {
                Instantiate(phase2EnemyPrefab, transform.position, Quaternion.identity);
            }
        }
        
        // CASE 2: timed out -> do nothing (skip spawn)
        else
        {
            Debug.LogWarning($"{gameObject.name} timed out in air, skipping phase 2 spawn.");
        }

        Destroy(gameObject);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, col.bounds.extents.y + groundCheckDistance);
    }
}
