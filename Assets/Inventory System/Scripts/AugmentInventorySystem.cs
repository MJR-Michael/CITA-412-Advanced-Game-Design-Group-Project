using UnityEngine;

public class AugmentInventorySystem : InventorySystem
{
    protected override void PickupItemAtNode(InventoryNode inventoryNode)
    {
        base.PickupItemAtNode(inventoryNode);

        //------------Add logic for removing the augment from the inventory here------------
        Debug.Log("Augment removed from augment inventory. We should probably remove its effects from the player...");
    }

    protected override void PlaceItemAtGridPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {
        //Maybe do a check to ensure the item placed is indeed an augment item, not something random

        base.PlaceItemAtGridPosition(originGridPosition, itemStructure);

        //------------Apply custom logic for adding an augment here------------
        Debug.Log("Augment added to teh augment inventory. We should probably add its effects to the player");
    }
}
