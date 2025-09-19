using System;
using UnityEngine;

public class GridObject
{
    public GridPosition gridPosition;
    public GridObject connectedTo;

    public bool isExplored;

    public bool isPartOfChamber;
    public bool isPartOfHallway;

    public bool isChamberPlaceable;
    public bool isHallwayPlaceable;

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

    public void ConnectToObject(GridObject other)
    {
        connectedTo = other;        
    }
    public void ResetConnection()
    {
        connectedTo = null;
    }

    public void ResetExploration()
    {
        isExplored = false;
        connectedTo = null;
    }
}
