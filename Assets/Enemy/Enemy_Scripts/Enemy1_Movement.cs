using UnityEngine;
using System.Collections;

public class Enemy1_Movement : MonoBehaviour
{
    [SerializeField] public float speed = 3f;
    [SerializeField] public float minMoveDuration = 1f;
    [SerializeField] public float maxMoveDuration = 2f;
    [SerializeField] public float minPauseDuration = 2f;
    [SerializeField] public float maxPauseDuration = 5f;


    private Vector3 moveDirection;
    private Transform targetPlayer;

    void Start()
    {
        targetPlayer = GetClosestPlayer();
        StartCoroutine(MoveRandomly());
    }

    void Update()
    {
        RotateTowardsTarget(targetPlayer);
    }

    IEnumerator MoveRandomly()
    {
        while (true)
        {
            moveDirection = GetRandomDirection();

            float moveDuration = GetRandomMoveDuration();
            float pauseDuration = GetRandomPauseDuration();

            float elapsedTime = 0f;

            // Move for a set duration
            while (elapsedTime < moveDuration)
            {
                transform.position += moveDirection * speed * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Pause before changing direction
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private Vector3 GetRandomDirection()
    {
        Vector3 dir = Random.onUnitSphere;
        return dir.normalized;
    }

    private float GetRandomMoveDuration()
    {
        return Random.Range(minMoveDuration, maxMoveDuration);
    }

    private float GetRandomPauseDuration()
    {
        return Random.Range(minPauseDuration, maxPauseDuration);
    }

    private Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }

        return closest;
    }

    private void RotateTowardsTarget(Transform target)
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }
}