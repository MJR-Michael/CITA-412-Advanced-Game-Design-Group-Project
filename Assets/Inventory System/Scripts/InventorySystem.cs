using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [SerializeField]
    InventoryNode inventoryNodePrefab;

    [SerializeField]
    GridLayoutGroup inventoryGridLayoutGroup;

    [SerializeField]
    int startingIventoryNodeCount = 25;

    [SerializeField]
    GameObject content;



    [Header("TESTING")]
    [SerializeField]
    ItemStructureSO testItemStructure;


    Dictionary<GridPosition, ItemStructureSO> itemsInInventory = new Dictionary<GridPosition, ItemStructureSO>();
    List<InventoryNode> inventoryNodes = new List<InventoryNode>();
    int inventoryLength;

    private void Awake()
    {
        inventoryLength = inventoryGridLayoutGroup.constraintCount;
        for (int i = 0; i < startingIventoryNodeCount; i++)
        {
            int gridPosX = i % inventoryLength; //Column position in grid
            int gridPosZ = i / inventoryLength; //Row position in grid
            GridPosition inventoryNodeGridPos = new GridPosition(gridPosX, gridPosZ);

            InventoryNode inventoryNodeObj = Instantiate(inventoryNodePrefab, content.transform);
            inventoryNodeObj.Initialize(this, inventoryNodeGridPos);
            inventoryNodes.Add(inventoryNodeObj);
        }

        DropItemAtGridPosition(new GridPosition(0, 0), testItemStructure);

    }

    public void OnInventoryNodeClicked(InventoryNode clickedInventoryNode)
    {
        Debug.Log($"{name} clicked");
    }

    bool CanPickupItemAtPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {
        return false;
    }

    bool CanPlaceItemAtPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {
        return false;
    }

    void PickupItemAtPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {

    }

    void DropItemAtGridPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {
        //Add item to inventory dictionary
        itemsInInventory.Add(originGridPosition, itemStructure);

        //Set up all inventory nodes for the item in the inventory
        foreach (ItemStructureSO.ItemStructureNode itemStructureNode in itemStructure.GetItemStructure())
        {
            //Get absolute grid position of this node in the invventory
            GridPosition absoluteGridPosition = itemStructureNode.GetGridPosition() + originGridPosition;

            //Get the inventory node at this grid position
            InventoryNode inventoryNode = inventoryNodes.Find(x => x.gridPosition == absoluteGridPosition);

            //Assign it to the given item structure position
            inventoryNode.itemGridPosition = originGridPosition;

            //Set its sprite
            inventoryNode.SetSprite(itemStructureNode.GetSprite());
        }
    }
}
