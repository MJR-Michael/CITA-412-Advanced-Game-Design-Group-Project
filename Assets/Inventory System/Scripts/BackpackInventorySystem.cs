using UnityEngine;

public class BackpackInventorySystem : InventorySystem
{
    //Allows for objects to be added to the backpack without needing a direct reference
    public static BackpackInventorySystem Instance;

    protected override void Awake()
    {
        //Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        base.Awake();
    }
}
