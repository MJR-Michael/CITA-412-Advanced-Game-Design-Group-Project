using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chamber
{
    GridPosition originGridPosition;
    List<GridPosition> connectedTo;
    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    Dictionary<GridPosition, GridPosition> hallwayConnectors;
    bool isBossChamber;

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
        connectedTo = new List<GridPosition>();
        this.isBossChamber = isBossChamber;

        //Debug.Log($"Current chamber; {originGridPosition}");

        //Debug.Log($"given hallway connectors: {hallwayConnectors.GetHashCode()}");
        //Debug.Log($"this hallway connectors: {this.hallwayConnectors.GetHashCode()}");

        //foreach(GridPosition gridPosition in hallwayConnectors.Keys.ToList())
        //{
        //    Debug.Log(gridPosition);
        //}
    }

    public bool UseHallwayConnector(GridPosition connectorGridPosition)
    {
        return hallwayConnectors.Remove(connectorGridPosition);
    }

    public void AddConnection(GridPosition otherOriginGridPosition)
    {
        connectedTo.Add(otherOriginGridPosition);
    }
    public void RemoveConnection(GridPosition otherOriginGridPosition)
    {
        connectedTo.Remove(otherOriginGridPosition);
    }
    public bool IsBossChamber() => isBossChamber;
    public List<GridPosition> ConnectTo() => connectedTo;
    /// <summary>
    /// Key = hallway connector to the chamber (absolute position). Value = the chamber that connects to a hallway (absolute position)
    /// </summary>
    public Dictionary<GridPosition, GridPosition> HallwayConnectors() => hallwayConnectors;
    public GridPosition OriginGridPosition => originGridPosition;
}