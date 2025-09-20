using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chamber
{
    GridPosition originGridPosition;
    List<GridPosition> positionsConnectedTo;


    Chamber pathfindingConnectedTo;

    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    Dictionary<GridPosition, GridPosition> _originalHallwayConnectors;
    Dictionary<GridPosition, GridPosition> hallwayConnectors;


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
        this.hallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        _originalHallwayConnectors = new Dictionary<GridPosition, GridPosition>(hallwayConnectors);
        positionsConnectedTo = new List<GridPosition>();
        this.isBossChamber = isBossChamber;
        isVisited = false;
        pathfindingConnectedTo = null;
        isConnectedToStart = false;
    }

    public bool UseHallwayConnector(GridPosition connectorGridPosition)
    {
        return hallwayConnectors.Remove(connectorGridPosition);
    }

    public void AddConnection(GridPosition otherOriginGridPosition)
    {
        positionsConnectedTo.Add(otherOriginGridPosition);
    }
    public void RemoveConnection(GridPosition otherOriginGridPosition)
    {
        positionsConnectedTo.Remove(otherOriginGridPosition);
    }
    public bool IsBossChamber() => isBossChamber;
    public List<GridPosition> ConnectTo() => positionsConnectedTo;
    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    public Dictionary<GridPosition, GridPosition> HallwayConnectors() => hallwayConnectors;

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
}