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

    [SerializeField]
    ItemStructureSO testItem2;

    [SerializeField]
    ItemStructureSO testItem3;

    Dictionary<GridPosition, ItemStructureSO> itemsInInventory = new Dictionary<GridPosition, ItemStructureSO>();
    List<InventoryNode> inventoryNodes = new List<InventoryNode>();
    int inventoryLength;

    private void Awake()
    {
        inventoryLength = inventoryGridLayoutGroup.constraintCount;
        AddNodesToInventory(startingIventoryNodeCount);

        if (CanPlaceItemAtGridPosition(new GridPosition(0,0), testItemStructure))
        {
            Debug.Log("Can place item at position. Placing it...");
            PlaceItemAtGridPosition(new GridPosition(0, 0), testItemStructure);
        }
        else
        {
            Debug.Log("Couldn't place item at position");
        }

        Debug.Log("Adding another item for testing...");
        if (CanPlaceItemAtGridPosition(new GridPosition(0, 0), testItemStructure))
        {
            Debug.Log("Can place item at position. Placing it...");
            PlaceItemAtGridPosition(new GridPosition(0, 0), testItemStructure);
        }
        else
        {
            Debug.Log("Couldn't place item at position");
        }

        Debug.Log("Adding item at valid position");

        if (CanPlaceItemAtGridPosition(new GridPosition(0, 1), testItemStructure))
        {
            Debug.Log("Can place item at position. Placing it...");
            PlaceItemAtGridPosition(new GridPosition(0, 1), testItemStructure);
        }
        else
        {
            Debug.Log("Couldn't place item at position");
        }

        Debug.Log("Adding some extra inventory slots...");

        AddNodesToInventory(5);

        PlaceItemAtGridPosition(new GridPosition(4, 1), testItem3);
    }

    private void Start()
    {
        Debug.Log("Giviong the player an item to start with");
        PlayerCursorInventory.Instance.TakeItem(testItem2);
    }

    public void AddNodesToInventory(int numOfNodesToAdd)
    {
        for (int i = 0; i < numOfNodesToAdd; i++)
        {
            //Get the grid position of this new node
            int gridPosX = inventoryNodes.Count % inventoryLength; //Column position in grid
            int gridPosZ = inventoryNodes.Count / inventoryLength; //Row position in grid

            GridPosition inventoryNodeGridPos = new GridPosition(gridPosX, gridPosZ);

            InventoryNode inventoryNodeObj = Instantiate(inventoryNodePrefab, content.transform);
            inventoryNodeObj.Initialize(this, inventoryNodeGridPos);
            inventoryNodes.Add(inventoryNodeObj);
        }
    }

    public void OnInventoryNodeClicked(InventoryNode clickedInventoryNode)
    {
        Debug.Log($"{name} clicked");

        //Handle the logic for clicking an inventory node
        if (PlayerCursorInventory.Instance.HasItem())
        {
            //Handle the logic for placing an item into a node


            //Get the item the player is holding
            ItemStructureSO itemStructure = PlayerCursorInventory.Instance.GetItem();

            //Check if the item can be placed at the clicked inventory node
            if (CanPlaceItemAtGridPosition(clickedInventoryNode.gridPosition, itemStructure))
            {
                //Place the item at the given position
                PlaceItemAtGridPosition(clickedInventoryNode.gridPosition, itemStructure);
                //Remove the item from the player cursor's inventory
                PlayerCursorInventory.Instance.RemoveItem();
            }
        }
        else
        {
            //Handle the logic for picking up an item from an inventory node


            //Check if you can pick up the item at the given item node
            if (CanPickupItemAtNode(clickedInventoryNode))
            {
                //Pick up the item
                PickupItemAtNode(clickedInventoryNode);
            }
        }
    }

    bool CanPickupItemAtNode(InventoryNode inventoryNode)
    {
        //If there is no item at the node, then there is nothing to pick up
        if (!inventoryNode.IsPartOfItem()) { return false; }

        //Player can pick up item at the given node
        return true;
    }

    bool CanPlaceItemAtGridPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
    {
        //Set up all inventory nodes for the item in the inventory
        foreach (ItemStructureSO.ItemStructureNode itemStructureNode in itemStructure.GetItemStructure())
        {
            //Get absolute grid position of this node in the invventory
            GridPosition absoluteGridPosition = itemStructureNode.GetGridPosition() + originGridPosition;

            //Get the inventory node at this grid position
            int inventoryNodeIndex = inventoryNodes.FindIndex(x => x.gridPosition == absoluteGridPosition);
            if (inventoryNodeIndex == -1)
            {
                //No index found; item structure goes out of bounds of the inventory
                return false;
            }
            InventoryNode inventoryNode = inventoryNodes[inventoryNodeIndex];

            //If the node is already assigned to an item, then the current item cannot be placed at this position
            if (inventoryNode.IsPartOfItem()) { return false; }
        }

        //All inventory nodes that the item will be assigned to are valid for placement
        return true;
    }

    /// <summary>
    /// Picks up the item at the given node. Assumes that the node can be picked up. NOTE: to test if an item can be picked up, use the method CanPickupItemAtNode
    /// </summary>
    /// <param name="inventoryNode">The node in question for item pickup</param>
    void PickupItemAtNode(InventoryNode inventoryNode)
    {
        //Get the origin position of the item that this inventory node is assigned to
        GridPosition originGridPosition = inventoryNode.GetItemGridPosition();

        //Get the item structure at this grid position
        ItemStructureSO itemStructure = itemsInInventory[originGridPosition];
        //Remove the item from the inventory
        itemsInInventory.Remove(originGridPosition);

        //Add the item to the player cursor's inventory
        PlayerCursorInventory.Instance.TakeItem(itemStructure);

        //Reset the nodes that this item was connected to
        foreach (ItemStructureSO.ItemStructureNode itemStructureNode in itemStructure.GetItemStructure())
        {
            //Get absolute grid position of this node in the invventory
            GridPosition absoluteGridPosition = itemStructureNode.GetGridPosition() + originGridPosition;

            //Get the inventory node at this grid position
            InventoryNode itemStructureInventoryNode = inventoryNodes.Find(x => x.gridPosition == absoluteGridPosition);

            //Reset the node's properties
            itemStructureInventoryNode.ResetInventoryNode();
        }
    }

    /// <summary>
    /// Places an item at the given grid position. Assumes that the position is valid. NOTE: to test if the position is valid, use the method CanPlaceItemAtGridPosition
    /// </summary>
    /// <param name="originGridPosition">The clicked position to place the item at</param>
    /// <param name="itemStructure">The type of item being placed at the position</param>
    void PlaceItemAtGridPosition(GridPosition originGridPosition, ItemStructureSO itemStructure)
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
