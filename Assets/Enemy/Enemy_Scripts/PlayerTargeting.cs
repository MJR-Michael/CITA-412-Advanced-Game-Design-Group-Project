using UnityEngine;

public static class PlayerTargeting
{
    public static Transform GetClosestPlayer(Vector3 fromPosition)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Player player in Player.AllPlayers)
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

    public static void RotateTowardsTarget(Transform self, Transform target, float rotationSpeed = 10f)
    {
        if (target == null) return;

        Vector3 direction = (target.position - self.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        self.rotation = Quaternion.Slerp(self.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
