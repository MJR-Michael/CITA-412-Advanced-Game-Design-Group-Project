using UnityEngine;

[System.Serializable]
public class ChamberNode
{
    [Tooltip("Reference the chamber design documentation for the grid position of the current node")]
    public GridPosition relativeChamberNodeGridPosition;
    [Tooltip("The relative position of the hallway connector from the chamebr grid position.\n\n" +
        "For example, if chamebr grid position is (1,0), and the hallway connector is (1,-1), enter (0,-1).\n\n" +
        "Hallway connector is 1 unit below the grid position")]
    public GridPosition[] hallwayConnectorPositions;
    public bool isPlayerSpawnPosition;
}