using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    //The actual map being generated
    [SerializeField]
    ChamberLayoutSO[] chamberLayouts;

    [SerializeField]
    GameObject tempChamberGridObject;

    [SerializeField]
    int mapLength = 10;

    [SerializeField]
    int mapWidth = 10;

    [SerializeField]
    int minNumOfChambers = 5;

    [SerializeField]
    int maxNumOfChambers = 100;

    GridObject[,] map;
    //Dictionary<GridPosition, GridObject> chamberPositions = new Dictionary<GridPosition, GridObject>();
    Dictionary<GridPosition, ChamberLayoutSO> chamberPositions = new Dictionary<GridPosition, ChamberLayoutSO>();
    Dictionary<GridPosition, GridObject> availableChamberPositions = new Dictionary<GridPosition, GridObject>();
    int mapArea;

    static GridPosition[] adjacentGridPositions =
    {
        new GridPosition(1,0),   //Right
        new GridPosition(0,1),   //Up
        new GridPosition(-1,0),  //Left
        new GridPosition(0,-1),  //Down
    };

    private void Awake()
    {
        map = new GridObject[mapWidth, mapLength];

        //Add the grid objects
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                //Make the grid position
                GridPosition gridPos = new GridPosition(i,j);

                map[i, j] = new GridObject(gridPos);
            }
        }

        mapArea = mapLength * mapWidth;

        PlaceChambersOnMap();
    }

    private void PlaceChambersOnMap()
    {
        //Add all available chamber positions onto the map
        for (int i = 0; i < mapLength; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                if (map[i, j].isChamberPlaceable)
                {
                    availableChamberPositions.Add(map[i, j].gridPosition, map[i, j]);
                }
            }
        }

        //Generate a number of chambers to add to the map
        int numOfChambersToPlace = Random.Range(minNumOfChambers, maxNumOfChambers + 1);

        List<GridPosition> availableGridPositionsForChambers = availableChamberPositions.Keys.ToList();

        //Loop through until all chambers are placed, or there are no more positions to place chambers
        while (numOfChambersToPlace > 0 && availableGridPositionsForChambers.Count > 0)
        {
            //Get a random chamber layout to place
            ChamberLayoutSO chamberLayout = GetRandomChamberLayout();

            //List of grid positions not yet tested for adding the current chamber
            List<GridPosition> gridPositionsToTest = GetListOfAvailableGridPositionsForChamber(chamberLayout, availableGridPositionsForChambers);
            bool chamberIsNotPlaced = true;

            //Try adding the chamber until out of grid positions to test.
            while (gridPositionsToTest.Count > 0 && chamberIsNotPlaced)
            {
                //Get random grid position for the origin of the chamber
                int randomChamberOriginPosition = Random.Range(0, gridPositionsToTest.Count);
                GridPosition originChamberNodeGridPosition = gridPositionsToTest[randomChamberOriginPosition];

                //Current grid position is no longer being tested for current chamber placement
                gridPositionsToTest.Remove(originChamberNodeGridPosition);

                //Try adding the chamber to the map
                if (ChamberCanBePlacedAtPosition(chamberLayout, originChamberNodeGridPosition))
                {
                    //Chamber can be added to map
                    chamberIsNotPlaced = false;

                    //Tell the map that the positions that this chamber takes up is no longer available for other chambers
                    AddChamberToMap(chamberLayout, originChamberNodeGridPosition, availableGridPositionsForChambers);
                }
            }

            if (chamberIsNotPlaced)
            {
                Debug.LogWarning("Warning: Tried adding chamber to map, but the chamber could not be placed!");
            }

            numOfChambersToPlace--;
        }

        //Place the chambers at the grid positions
        foreach(KeyValuePair<GridPosition, ChamberLayoutSO> chamberKeyValuePair in chamberPositions)
        {
            GridPosition gridPosition = chamberKeyValuePair.Key;
            Vector3 chamberWorldPos = GetWorldPositionFromGridPosition(gridPosition);

            Instantiate(chamberPositions[gridPosition].chamberPrefab, chamberWorldPos, Quaternion.identity);
        }

        Debug.Log($"Remaining chamber to place: {numOfChambersToPlace}");
        Debug.Log($"Remaining chamber positions: {availableGridPositionsForChambers.Count}");


        Debug.Log("Map generation complete!");
        Debug.Log("Chambers were generated at positions:");
        foreach (GridPosition gridPosition in chamberPositions.Keys)
        {
            Debug.Log(gridPosition);
        }

    }


    /// <summary>
    /// Adds chamber to the map and handles logic for data storage for chamber posiitons. Assumes that the chamber is fully placeable at the given position
    /// </summary>
    /// <param name="chamberLayout"></param>
    /// <param name="originGridPosition"></param>
    private void AddChamberToMap(ChamberLayoutSO chamberLayout, GridPosition originGridPosition, List<GridPosition> availableGridPositionForChambers)
    {
        //Add the chamber to the dictionary of chamber positions
        chamberPositions[originGridPosition] = chamberLayout;

        //Tell all adjacent chamber positions and the positions of the chamber that they are no longer placeable for other chambers
        List<GridPosition> absoluteChamberGridPositions = GetAbsoluteChamberGridPositions(chamberLayout.GetChamberLayoutGridPositions(), originGridPosition);
        foreach(GridPosition chamberGridPosition in absoluteChamberGridPositions)
        {
            //Current grid position is no longer chamber placeable.
            availableChamberPositions[chamberGridPosition].MakeObjectPartOfChamber();

            availableGridPositionForChambers.Remove(chamberGridPosition);
            availableChamberPositions.Remove(chamberGridPosition);

            //Loop through all adjacent grid positions
            foreach(GridPosition relativeAdjacentGridPosition in adjacentGridPositions)
            {
                GridPosition absoluteAdjacentGridPosition = relativeAdjacentGridPosition + chamberGridPosition;
                //Test if the adjacent position is a chamber position. Skip if it is
                if (absoluteChamberGridPositions.Contains(absoluteAdjacentGridPosition)) { continue; }

                //remove from available grid positions for chambers
                availableGridPositionForChambers.Remove(absoluteAdjacentGridPosition);
                availableChamberPositions[absoluteAdjacentGridPosition].SetChamberPlaceable(false);
                availableChamberPositions.Remove(absoluteAdjacentGridPosition);
            }
        }
    }

    /// <summary>
    /// Filters out positions on the map where the chambe cannot be placed. Only contains positions where the chamber can be placed
    /// </summary>
    /// <param name="chamberLayout">the scriptable object for the chamber</param>
    /// <param name="availableGridPositionsOnMap">the list of grid positions on the map that any chamber may be placed at</param>
    /// <returns></returns>
    private List<GridPosition> GetListOfAvailableGridPositionsForChamber(ChamberLayoutSO chamberLayout, List<GridPosition> availableGridPositionsOnMap)
    {
        List<GridPosition> availableGridPositions = new List<GridPosition>();

        foreach (GridPosition availableGridPosition in availableGridPositionsOnMap)
        {
            availableGridPositions.Add(availableGridPosition);
        }

        //Height & Length of chamber
        int heightOfChamber = chamberLayout.GetHeightOfChamber();
        int legnthOfChamber = chamberLayout.GetLengthOfChamber();

        //Determine the max x,z index for the current chamber on the map
        int maxXIndexForChamber = (mapLength - 1) - legnthOfChamber;
        int maxZIndexForChamber = (mapWidth - 1) - heightOfChamber;

        //Filter out unavailable grid positions
        List<GridPosition> gridPositionsToRemove = new List<GridPosition>();

        foreach(GridPosition gridPosition in availableGridPositions)
        {
            if (gridPosition.x > maxXIndexForChamber)
            {
                //not enough length for chamber
                gridPositionsToRemove.Add(gridPosition);
                continue;
            }
            if (gridPosition.z > maxZIndexForChamber)
            {
                //Not enough height for chamber
                gridPositionsToRemove.Add(gridPosition);
                continue;
            }
        }

        foreach(GridPosition gridPosition in gridPositionsToRemove)
        {
            availableGridPositions.Remove(gridPosition);
        }

        return availableGridPositions;
    }

    Vector3 GetWorldPositionFromGridPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z);
    }

    ChamberLayoutSO GetRandomChamberLayout()
    {
        if (chamberLayouts == null)
        {
            Debug.LogWarning("Warning: chamber layouts was not initialized!");
            return null;
        }
        if (chamberLayouts.Length == 0)
        {
            Debug.LogWarning("Warning: no chamber layouts given to create map!");
            return null;
        }

        int numOfChamberLayouts = chamberLayouts.Length;
        int randomChamberIndex = Random.Range(0, numOfChamberLayouts);
        return chamberLayouts[randomChamberIndex];
    }

    /// <summary>
    /// Method to get the positions of the chamber's hallway connector positions on the grid.
    /// </summary>
    /// <param name="chamberLayout">The sscriptable object of the chamebr</param>
    /// <param name="originGridPositionOnMap">the position of the chamber's origin on the real map</param>
    /// <returns>the positions of the hallway connector positions</returns>
    List<GridPosition> GetChamberHallwayConnectorPositionsInGrid(ChamberLayoutSO chamberLayout, GridPosition originGridPositionOnMap)
    {
        List<GridPosition> chamberHallwayPositions = new List<GridPosition>();

        //Loop through each chamber node (0,0), (0,1), ...
        for (int i = 0; i < chamberLayout.chamberNodes.Length; i++)
        {
            ChamberNode chamberNode = chamberLayout.chamberNodes[i];

            //Check if node has any hallway connectors
            if (chamberNode.hallwayConnectorPositions == null)
            {
                //Hallway connectors doesn't exist
                Debug.LogWarning($"Warning: Chamber {chamberLayout.name} is missing hallway connector position array! Something may be wrong.");
                continue;
            }
            if (chamberNode.hallwayConnectorPositions.Length == 0)
            {
                //No hallway connectors to add
                continue;
            }

            //Get the real grid position of the hallway connector
            GridPosition chamberNodeGridPosition = chamberNode.chamberGridPosition + originGridPositionOnMap;

            //Loop through each hallway connector position for the current chamber node
            for (int j = 0; j < chamberNode.hallwayConnectorPositions.Length; j++)
            {
                GridPosition hallwayConnectorGridPosition = chamberNodeGridPosition + chamberNode.hallwayConnectorPositions[j];

                //Add the chamber hallway connector position (real position on grid) to list
                chamberHallwayPositions.Add(hallwayConnectorGridPosition);
            }
        }

        return chamberHallwayPositions;
    }

    /// <summary>
    /// Tests to see if the chamber can be placed at the given grid position for the chamber's origin position
    /// </summary>
    /// <param name="chamberLayout">The scriptable object for the chamber</param>
    /// <param name="originGridPosition">The origin of the chamber on the map</param>
    /// <returns>True if chamber fits at the given position. False otherwise.</returns>
    bool ChamberCanBePlacedAtPosition(ChamberLayoutSO chamberLayout, GridPosition originGridPosition)
    {
        List<GridPosition> absoluteGridPositions = GetAbsoluteChamberGridPositions(chamberLayout.GetChamberLayoutGridPositions(), originGridPosition);

        foreach (GridPosition gridPositionOnMap in absoluteGridPositions)
        {
            //Test if this grid position is available
            if (!availableChamberPositions.ContainsKey(gridPositionOnMap))
            {
                //Grid position does not exist
                return false;
            }

            if (!availableChamberPositions[gridPositionOnMap].isChamberPlaceable)
            {
                //the position is not placeable; chamber cannot be placed here
                return false;
            }
        }

        //All grid positions passed the test. The chamber is placeable at this position
        return true;
    }

    List<GridPosition> GetAbsoluteChamberGridPositions(List<GridPosition> relativeGridPositions, GridPosition originGridPosition)
    {
        List<GridPosition> absoluteGridPositions = new List<GridPosition>();

        foreach (GridPosition relativeGridPosition in relativeGridPositions)
        {
            absoluteGridPositions.Add(relativeGridPosition + originGridPosition);
        }

        return absoluteGridPositions;
    } 
}
