using UnityEngine;

public class DeathNothing : MonoBehaviour, IDeathBehavior
{
    public void OnDeath(Enemy enemy)
    {
        Debug.Log($"{enemy.name} died, but nothing happens.");
        Destroy(gameObject, 1f);
    }
}
