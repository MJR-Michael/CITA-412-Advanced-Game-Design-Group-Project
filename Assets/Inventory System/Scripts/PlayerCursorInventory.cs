using System;
using UnityEngine;

public class PlayerCursorInventory : MonoBehaviour
{
    public static PlayerCursorInventory Instance;

    public ItemStructureSO itemStructure;

    private void Awake()
    {
        //Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasItem()
    {
        return itemStructure == null ? false : true;
    }

    public void TakeItem(ItemStructureSO itemStructure)
    {
        Debug.Log($"Player picked up item: {itemStructure.GetItemName()}");
        this.itemStructure = itemStructure;
    }

    public void RemoveItem()
    {
        itemStructure = null;
    }

    public ItemStructureSO GetItem() => itemStructure;
}
