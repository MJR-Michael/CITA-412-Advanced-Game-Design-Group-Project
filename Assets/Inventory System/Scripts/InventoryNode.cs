using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryNode : Button
{
    public RectTransform rectTransform { get; private set; }
    InventorySystem inventorySystem;
    public GridPosition gridPosition{get; private set;}

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        //Handle button clicked events here
    }

    public void Initialize(InventorySystem inventorySystem, GridPosition gridPosition)
    {
        this.inventorySystem = inventorySystem;
        this.gridPosition = gridPosition;

        name = $"{gridPosition} Inventory Slot";
    }
}
