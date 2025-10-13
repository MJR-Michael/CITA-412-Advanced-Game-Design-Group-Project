using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryStructureSO", menuName = "ScriptableObject/InventoryStructureSO")]
public class ItemStructureSO : ScriptableObject
{
    [System.Serializable]
    public class ItemStructureNode
    {
        [SerializeField]
        [Tooltip("ENTER TOOLTIP")]
        GridPosition gridPosition;

        [SerializeField]
        [Tooltip("ENTER TOOLTIP")]
        Sprite sprite;

        public GridPosition GetGridPosition() => gridPosition;
        public Sprite GetSprite() => sprite;

    }

    [SerializeField]
    string itemName;

    [SerializeField]
    string itemDescription;

    [SerializeField]
    ItemStructureNode[] itemStructure;

    public string GetItemName() => itemName;
    public string GetItemDescription() => itemDescription;
    public ItemStructureNode[] GetItemStructure() => itemStructure;
}
