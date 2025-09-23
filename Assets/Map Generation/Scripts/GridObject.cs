using System;
using UnityEngine;

public class GridObject
{
    public GridPosition gridPosition;
    public GridObject connectedTo;
    public bool isPartOfChamber;
    public bool isPartOfAChamberHallway;
    public bool isPartOfHallway;

    public bool isChamberPlaceable;
    public bool isHallwayPlaceable;

    public int lastVisitedID;
    public bool isClosed = false;

    //Jump Point Search / A* Requirements
    /// <summary>
    /// gCost + hCost
    /// </summary>
    public int fCost;
    /// <summary>
    /// The number of "steps" from start node
    /// </summary>
    public int gCost = int.MaxValue;
    /// <summary>
    /// The Manhattan distance from start node (delta x + delta y)
    /// </summary>
    public int hCost;


    public GridObject(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;

        isPartOfChamber = false;
        isPartOfHallway = false;

        isChamberPlaceable = true;
        isHallwayPlaceable = true;
    }
    public void MakeObjectPartOfChamber()
    {
        isPartOfChamber = true;
        isPartOfHallway = false;
        isChamberPlaceable = false;
        isHallwayPlaceable = false;
    }
    public void SetChamberPlaceable(bool isChamberPlaceable)
    {
        this.isChamberPlaceable = isChamberPlaceable;
        isPartOfChamber = false;
    }
    public void MakeObjectAHallway()
    {
        isChamberPlaceable = false;
        isHallwayPlaceable = false;
        isPartOfHallway = true;
        isPartOfChamber = false;
    }
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    public void ResetPathfindingProperties()
    {
        gCost = int.MaxValue;
        connectedTo = null;
        isClosed = false;
    }
}
