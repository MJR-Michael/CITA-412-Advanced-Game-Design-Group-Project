using UnityEngine;

public class Enemy2_Movement : MonoBehaviour
{
    public float amplitude = 0.7f;
    public float frequency = 0.2f;

    public float horizontalSpeed = 0.1f;
    public float horizontalRange = 2f;
    public float horizontalRandomFactor = 5f;

    public float rotationSpeed = 5f;

    public Vector3 startPosition;
    private Vector3 currentHorizontalOffset;
    private Vector3 horizontalTarget;

    private Transform targetPlayer;

    [HideInInspector]
    public bool canMove = true; // <-- Add this

    private void Start()
    {
        startPosition = transform.position;

        horizontalTarget = new Vector3(
            Random.Range(-horizontalRange, horizontalRange),
            0f,
            Random.Range(-horizontalRange, horizontalRange)
        );

        targetPlayer = PlayerTargeting.GetClosestPlayer(transform.position);
    }

    private void Update()
    {
        if (!canMove) return; // <-- Skip movement if disabled

        // Vertical movement
        float verticalOffset = Mathf.Sin(Time.time * frequency * Mathf.PI * 2) * amplitude;

        // Horizontal wandering
        currentHorizontalOffset = Vector3.Lerp(
            currentHorizontalOffset,
            horizontalTarget,
            Time.deltaTime * horizontalSpeed * Random.Range(0.5f, horizontalRandomFactor)
        );

        if (Vector3.Distance(new Vector3(currentHorizontalOffset.x, 0, currentHorizontalOffset.z), horizontalTarget) < 0.05f)
        {
            horizontalTarget = new Vector3(
                Random.Range(-horizontalRange, horizontalRange),
                0f,
                Random.Range(-horizontalRange, horizontalRange)
            );
        }

        transform.position = startPosition + new Vector3(currentHorizontalOffset.x, verticalOffset, currentHorizontalOffset.z);

        if (targetPlayer != null)
        {
            PlayerTargeting.RotateTowardsTarget(transform, targetPlayer, rotationSpeed);
        }
        else
        {
            targetPlayer = PlayerTargeting.GetClosestPlayer(transform.position);
        }
    }
}
