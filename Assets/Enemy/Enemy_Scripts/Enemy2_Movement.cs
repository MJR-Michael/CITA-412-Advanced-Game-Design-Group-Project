using UnityEngine;

public class Enemy2_Movement : MonoBehaviour
{
    public float amplitude = 0.7f;          // How far up and down
    public float frequency = 0.2f;          // How fast it moves up/down

    public float horizontalSpeed = 0.1f;    // Speed of horizontal wandering
    public float horizontalRange = 2f;      // Max X/Z offset
    public float horizontalRandomFactor = 5f;

    public float rotationSpeed = 5f;

    private Vector3 startPosition;
    private Vector3 currentHorizontalOffset;
    private Vector3 horizontalTarget;

    private Transform targetPlayer;

    private void Start()
    {
        startPosition = transform.position;

        // Initialize first horizontal target
        horizontalTarget = new Vector3(
            Random.Range(-horizontalRange, horizontalRange),
            0f,
            Random.Range(-horizontalRange, horizontalRange)
        );

        targetPlayer = PlayerTargeting.GetClosestPlayer(transform.position);
    }

    private void Update()
    {
        // Vertical movement using sine wave 
        float verticalOffset = Mathf.Sin(Time.time * frequency * Mathf.PI * 2) * amplitude;

        // Horizontal wandering
        currentHorizontalOffset = Vector3.Lerp(
            currentHorizontalOffset,
            horizontalTarget,
            Time.deltaTime * horizontalSpeed * Random.Range(0.5f, horizontalRandomFactor)
        );

        // Pick new horizontal target if reached
        if (Vector3.Distance(new Vector3(currentHorizontalOffset.x, 0, currentHorizontalOffset.z), horizontalTarget) < 0.05f)
        {
            horizontalTarget = new Vector3(
                Random.Range(-horizontalRange, horizontalRange),
                0f,
                Random.Range(-horizontalRange, horizontalRange)
            );
        }

        // Apply final position
        transform.position = startPosition + new Vector3(currentHorizontalOffset.x, verticalOffset, currentHorizontalOffset.z);

        if (targetPlayer != null)
        {
            PlayerTargeting.RotateTowardsTarget(transform, targetPlayer, rotationSpeed);
        }
        else
        {
            // If the target player was destroyed, pick a new one
            targetPlayer = PlayerTargeting.GetClosestPlayer(transform.position);
        }
    }
}
