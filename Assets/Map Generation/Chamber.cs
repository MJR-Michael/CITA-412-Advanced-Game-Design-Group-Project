using System.Collections.Generic;

public class Chamber
{
    GridPosition originGridPosition;
    List<Chamber> connectedTo;


    Chamber pathfindingConnectedTo;

    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    Dictionary<GridPosition, GridPosition> _originalHallwayConnectors;
    Dictionary<GridPosition, GridPosition> hallwayConnectors;

    int lastVisitID = 0;
    bool isBossChamber;
    bool isVisited;
    bool isConnectedToStart;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="originGridPosition">absolute position of chamber's origin</param>
    /// <param name="hallwayConnectors">Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)</param>
    /// <param name="isBossChamber"></param>
    public Chamber(GridPosition originGridPosition, Dictionary<GridPosition, GridPosition> hallwayConnectors, bool isBossChamber)
    {
        this.originGridPosition = originGridPosition;
        this.isBossChamber = isBossChamber;

        pathfindingConnectedTo = null;

        isVisited = false;
        isConnectedToStart = false;

        this.hallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        _originalHallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        connectedTo = new List<Chamber>();
    }

    public bool UseHallwayConnector(GridPosition connectorGridPosition)
    {
        return hallwayConnectors.Remove(connectorGridPosition);
    }

    public void AddConnection(Chamber otherChamber)
    {
        connectedTo.Add(otherChamber);
    }
    public void RemoveConnection(Chamber otherChamber)
    {
        connectedTo.Remove(otherChamber);
    }
    public bool IsBossChamber() => isBossChamber;

    public bool IsConnectedTo(Chamber givenChamber) => connectedTo.Contains(givenChamber);
    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    public Dictionary<GridPosition, GridPosition> HallwayConnectors() => hallwayConnectors;

    public int LastVisitID() => lastVisitID;
    public void SetLastVisitID(int newID) => lastVisitID = newID;

    public bool IsVisited() => isVisited;

    public void SetIsVisited(bool isVisited)
    {
        this.isVisited = isVisited;
    }

    public void ResetVisitStatus()
    {
        SetIsVisited(false);
    }

    public void AddBackHallwayConnector(GridPosition hallwayConnector, GridPosition chamberHallwayConnector)
    {
        if (_originalHallwayConnectors.ContainsKey(hallwayConnector))
        {
            hallwayConnectors.Add(hallwayConnector, chamberHallwayConnector);
        }
    }

    public GridPosition OriginGridPosition => originGridPosition;

    public Chamber PathfindingConnectedTo() => pathfindingConnectedTo;
    public void SetPathfindingConnectedTo(Chamber theChamberItIsConnectedTo)
    {
        pathfindingConnectedTo = theChamberItIsConnectedTo;
    }

    public void SetIsConnectedToStart(bool isConnectedToStart)
    {
        this.isConnectedToStart = isConnectedToStart;
    }

    public bool IsConnectedToStartChamber() => isConnectedToStart;

    public int GetNumOfHallwayConnectors() => hallwayConnectors.Count;
}