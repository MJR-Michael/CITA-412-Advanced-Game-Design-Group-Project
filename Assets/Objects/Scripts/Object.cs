using System;
using UnityEngine;

public class Object : MonoBehaviour
{
    [SerializeField]
    Health health;

    [SerializeField]
    Destructible destructiblePrefab;

    private void Awake()
    {
        if (health == null)
        {
            Debug.LogError("No health script attached to game object");
        }

        if (destructiblePrefab == null)
        {
            Debug.LogWarning("No destructiblePrefab script attached to game object");
        }

        health.OnDeath += HandleObjectDestroyed;
    }

    private void HandleObjectDestroyed(GameObject sender, Health health, DamageType type)
    {
        if (destructiblePrefab == null)
        {
            Debug.LogWarning($"Warning: {gameObject} has no destructible prefab reference!");
            Destroy(gameObject);
            return;
        }


        //Create the destructible object
        Destructible destructibleObj = Instantiate(
            destructiblePrefab,
            transform.position,
            transform.rotation
            );

        float forceMagnitude = 1000f;

        //If being hit by a specific type of object, handle the logic here
        //for example, if the object that hit this was a projectile, get its applied force on hit
        //and change the value of force magnitude to it

        destructibleObj.Initialize(sender.transform.forward, forceMagnitude);

        //Clean up this object
        Destroy(gameObject);
    }
}
