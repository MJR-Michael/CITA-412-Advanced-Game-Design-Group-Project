using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class InventoryNode : Button
{
    public RectTransform rectTransform { get; private set; }
    public GridPosition gridPosition {get; private set;}
    public GridPosition itemGridPosition {get; set;}

    [SerializeField]
    Image itemImageOverlay;

    InventorySystem inventorySystem;
    Sprite defaultSprite;

    protected override void Awake()
    {
        base.Awake();

        defaultSprite = itemImageOverlay.sprite;

        rectTransform = GetComponent<RectTransform>();
        onClick.AddListener(HandleButtonClicked);
    }

    private void HandleButtonClicked()
    {
        //Handle button clicked events here
        inventorySystem.OnInventoryNodeClicked(this);
    }

    public void Initialize(InventorySystem inventorySystem, GridPosition gridPosition)
    {
        this.inventorySystem = inventorySystem;
        this.gridPosition = gridPosition;

        name = $"{gridPosition} Inventory Slot";
        itemGridPosition = GridPosition.Invalid;
        SetSpriteDefault();
    }

    public void SetSprite(Sprite sprite)
    {
        itemImageOverlay.sprite = sprite;
        itemImageOverlay.color = new Color(255,255,255,255);
    }

    public void SetSpriteDefault()
    {
        itemImageOverlay.sprite = defaultSprite;
        itemImageOverlay.color = new Color(0, 0, 0, 0);
    }

    public bool IsPartOfItem()
    {
        return itemGridPosition == GridPosition.Invalid ? false : true;
    }

    public GridPosition GetItemGridPosition() => itemGridPosition;

    public void ResetInventoryNode()
    {
        SetSpriteDefault();
        itemGridPosition = GridPosition.Invalid;
    }
}
