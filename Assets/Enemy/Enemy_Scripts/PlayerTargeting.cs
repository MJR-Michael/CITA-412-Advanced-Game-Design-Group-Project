using UnityEngine;

public static class PlayerTargeting
{
    //Finds the closest player to the given position.
    public static Transform GetClosestPlayer(Vector3 fromPosition)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(fromPosition, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player.transform;
            }
        }

        return closest;
    }

    // Rotates a transform to face the target smoothly.
    public static void RotateTowardsTarget(Transform self, Transform target, float rotationSpeed = 10f)
    {
        if (target == null) return;

        Vector3 direction = (target.position - self.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        self.rotation = Quaternion.Slerp(self.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
