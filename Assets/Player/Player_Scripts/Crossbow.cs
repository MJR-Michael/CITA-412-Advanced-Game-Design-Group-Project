using UnityEngine;

public class Crossbow : MonoBehaviour
{
    public Transform crossbowFirePoint;
    public GameObject crossbowProjectile;
    public float crossbowFireRate = 1f;

    private float crossbowCooldown = 0f;

    private void Update()
    {
        crossbowCooldown -= Time.deltaTime;

        if ((InputManager.Instance.GetPrimaryFireInput() > 0) && (crossbowCooldown <= 0f))
        {
            Shoot();
            crossbowCooldown = 1f / crossbowFireRate;
        }

    }

    private void Shoot()
    {
        GameObject bolt = Instantiate(crossbowProjectile, crossbowFirePoint.position, crossbowFirePoint.rotation);
    }
}
