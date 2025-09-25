using System;
using System.Collections.Generic;

public class Chamber
{
    List<Chamber> connectedTo;
    GridPosition originGridPosition;
    ChamberLayoutSO chamberLayoutSO;

    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    Dictionary<GridPosition, GridPosition> _originalHallwayConnectors;

    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    Dictionary<GridPosition, GridPosition> hallwayConnectors;

    bool isBossChamber;
    bool isVisited;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="originGridPosition">absolute position of chamber's origin</param>
    /// <param name="hallwayConnectors">Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)</param>
    /// <param name="isBossChamber"></param>
    public Chamber(GridPosition originGridPosition, ChamberLayoutSO chamberLayoutSO, Dictionary<GridPosition, GridPosition> hallwayConnectors, bool isBossChamber)
    {
        this.hallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        _originalHallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        connectedTo = new List<Chamber>();

        this.originGridPosition = originGridPosition;
        this.isBossChamber = isBossChamber;
        this.chamberLayoutSO = chamberLayoutSO;
        isVisited = false;
    }
    public GridPosition GetPositionOfChamberConnectorFromHallwayPosition(GridPosition hallwayConnectorPosition)
    {
        if (_originalHallwayConnectors.ContainsKey(hallwayConnectorPosition))
        {
            return _originalHallwayConnectors[hallwayConnectorPosition];
        }

        return GridPosition.Invalid;
    }
    public bool UseHallwayConnector(GridPosition connectorGridPosition)
    {
        return hallwayConnectors.Remove(connectorGridPosition);
    }
    public void AddConnection(Chamber otherChamber) => connectedTo.Add(otherChamber);
    public bool IsBossChamber() => isBossChamber;
    public bool IsConnectedTo(Chamber givenChamber) => connectedTo.Contains(givenChamber);
    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    public Dictionary<GridPosition, GridPosition> HallwayConnectors() => hallwayConnectors;
    public bool IsVisited() => isVisited;
    public void SetIsVisited(bool isVisited) => this.isVisited = isVisited;

    public bool ContainsHallwayConnector(GridPosition gridPosition)
    {
        return hallwayConnectors.ContainsKey(gridPosition);
    }

    public GridPosition GetPlayerSpawnPosition()
    {
        //Get the spawn position from the prefab
        return chamberLayoutSO.GetSpawnPosition() + originGridPosition;
    }

    public object GetChamberLayoutSO() => chamberLayoutSO;
}