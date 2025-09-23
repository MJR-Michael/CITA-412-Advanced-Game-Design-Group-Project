using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chamber Layout", menuName = "ScriptableObject/ChamberLayout")]
public class ChamberLayoutSO : ScriptableObject
{
    [Tooltip("Reference the chamebr design documentation for the positions of each grid position and the hallway connectors")]
    public ChamberNode[] chamberNodes;

    public GameObject chamberPrefab;

    /// <summary>
    /// returns the index of the highest grid position (including hallway connectors)
    /// </summary>
    /// <returns></returns>
    public int GetHeightOfChamber()
    {
        int highestIndex = 0;

        for (int i = 0; i < chamberNodes.Length; i++)
        {
            ChamberNode currentChamberNode = chamberNodes[i];

            int heightIndexOfChamber = currentChamberNode.relativeChamberNodeGridPosition.z;
            //Get the tallest height based on hallway connectors, if applicable
            for ( int j = 0; j < currentChamberNode.hallwayConnectorPositions.Length; j++)
            {
                int heightIndexOfHallwayConnector = currentChamberNode.hallwayConnectorPositions[j].z;
                if (heightIndexOfChamber + heightIndexOfHallwayConnector > heightIndexOfChamber)
                {
                    heightIndexOfChamber += heightIndexOfHallwayConnector;
                }
            }

            //Test if height of current chamber node is higher than the current highest indes
            if (heightIndexOfChamber > highestIndex)
            {
                highestIndex = heightIndexOfChamber;
            }
        }

        return highestIndex + 1;
    }
    public int GetLengthOfChamber()
    {
        int rightMostIndex = 0;

        for (int i = 0; i < chamberNodes.Length; i++)
        {
            ChamberNode currentChamberNode = chamberNodes[i];

            int rightMostIndexOfChamber = currentChamberNode.relativeChamberNodeGridPosition.x;
            //Get the tallest height based on hallway connectors, if applicable
            for (int j = 0; j < currentChamberNode.hallwayConnectorPositions.Length; j++)
            {
                int rightMostIndexOfHallwayConnector = currentChamberNode.hallwayConnectorPositions[j].x;
                if (rightMostIndexOfChamber + rightMostIndexOfHallwayConnector > rightMostIndexOfChamber)
                {
                    rightMostIndexOfChamber += rightMostIndexOfHallwayConnector;
                }
            }

            //Test if height of current chamber node is higher than the current highest indes
            if (rightMostIndexOfChamber > rightMostIndex)
            {
                rightMostIndex = rightMostIndexOfChamber;
            }
        }

        return rightMostIndex + 1;
    }
    public List<GridPosition> GetChamberLayoutGridPositions()
    {
        List<GridPosition> gridPositions = new List<GridPosition>();

        foreach (ChamberNode chamberNode in chamberNodes)
        {
            gridPositions.Add(chamberNode.relativeChamberNodeGridPosition);
        }

        return gridPositions;
    }
    public Dictionary<GridPosition, GridPosition> GetAbsoluteHallwayConnectorPositions(GridPosition originGridPosition)
    {
        Dictionary<GridPosition, GridPosition> hallwayConnectors = new Dictionary<GridPosition, GridPosition>();

        foreach (ChamberNode chamberNode in chamberNodes)
        {
            //Conditions where there is no hallway connector positions
            if (chamberNode.hallwayConnectorPositions == null)
            {
                continue;
            }
            if (chamberNode.hallwayConnectorPositions.Length == 0)
            {
                continue;
            }

            //Get the absolute position for the current node
            GridPosition absoluteChamberNodePosition = chamberNode.relativeChamberNodeGridPosition + originGridPosition;

            foreach (GridPosition relativeHallwayConnectorPosition in chamberNode.hallwayConnectorPositions)
            {
                //Get the absolute position of the hallway connector position
                GridPosition absoluteHallwayConnectorPosition = absoluteChamberNodePosition + relativeHallwayConnectorPosition;
                //Store the position in the dicitonary
                hallwayConnectors[absoluteHallwayConnectorPosition] = absoluteChamberNodePosition;
            }
        }

        return hallwayConnectors;
    }
}
