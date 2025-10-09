using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    }
}
