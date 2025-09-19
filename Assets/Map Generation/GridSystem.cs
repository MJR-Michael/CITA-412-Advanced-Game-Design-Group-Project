using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    //The actual map being generated
    [SerializeField]
    ChamberLayoutSO[] chamberLayouts;

    [SerializeField]
    ChamberLayoutSO[] bossRoomLayouts;

    [SerializeField]
    GameObject tempChamberGridObject;

    [SerializeField]
    GameObject tempHallwayGridObject;

    [SerializeField]
    int mapLength = 10;

    [SerializeField]
    int mapWidth = 10;

    [SerializeField]
    int minNumOfChambers = 5;

    [SerializeField]
    int maxNumOfChambers = 100;

    [SerializeField]
    bool debugging;

    Dictionary<GridPosition, GridObject> map = new Dictionary<GridPosition, GridObject>();
    //Dictionary<GridPosition, GridObject> chamberPositions = new Dictionary<GridPosition, GridObject>();
    Dictionary<GridPosition, ChamberLayoutSO> chamberPositions = new Dictionary<GridPosition, ChamberLayoutSO>();
    Dictionary<GridPosition, GridObject> availableChamberPositions = new Dictionary<GridPosition, GridObject>();
    List<Chamber> chambers = new List<Chamber>();
    int mapArea;

    Chamber startingChamberPosition;
    Chamber bossChamber;

    static GridPosition[] adjacentGridPositions =
    {
        new GridPosition(1,0),   //Right
        new GridPosition(0,1),   //Up
        new GridPosition(-1,0),  //Left
        new GridPosition(0,-1),  //Down
    };

    private void Awake()
    {
        Debug.Log("Generating map");

        //Add the grid objects
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                //Make the grid position
                GridPosition gridPos = new GridPosition(i,j);

                map[gridPos] = new GridObject(gridPos);
            }
        }

        mapArea = mapLength * mapWidth;

        //Add all available chamber positions onto the map
        for (int i = 0; i < mapLength; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                GridPosition currentPos = new GridPosition(i, j);
                if (map[currentPos].isChamberPlaceable)
                {
                    availableChamberPositions.Add(currentPos, map[currentPos]);

                    //debug testing
                    if (debugging)
                    {
                        //Debug.Log($"Chamebr added at {i}, {j}");
                    }
                }
            }
        }

        float timeMapStartedGenerating = Time.realtimeSinceStartup;

        //Place chambers
        PlaceBossChamberOnMap();
        float timeToGenerateBossChamber = Time.realtimeSinceStartup - timeMapStartedGenerating;

        PlaceChambersOnMap();
        float timeToGenerateChambers = Time.realtimeSinceStartup - timeToGenerateBossChamber;

        InitializeChamberObjects();

        ConnectChambers();
        float timeToGenerateConnectors = Time.realtimeSinceStartup - timeToGenerateChambers;

        float timeToGenerateMap = Time.realtimeSinceStartup - timeMapStartedGenerating;

        if (debugging)
        {
            Debug.Log($"Time to generate map: {timeToGenerateMap}s");
            Debug.Log($"Time to generate boss chamber: {timeToGenerateBossChamber}");
            Debug.Log($"Time to generate other chambers: {timeToGenerateChambers}");
            Debug.Log($"Time to generate connectors: {timeToGenerateConnectors}");
        }
    }

    private void InitializeChamberObjects()
    {
        //Chamber has an origin, hallway connectors, connections to other chambers, and a flag for if it's a boss chamber
        foreach(KeyValuePair<GridPosition, ChamberLayoutSO> chamberPair in chamberPositions)
        {
            //Get the hallway connectors
            Dictionary<GridPosition, GridPosition> hallwayConnectors = chamberPair.Value.GetAbsoluteHallwayConnectorPositions(chamberPair.Key);
            bool isBossChamber = bossRoomLayouts.Contains(chamberPair.Value);

            //Generate the chamber object
            Chamber chamber = new Chamber(chamberPair.Key, hallwayConnectors, isBossChamber);

            //Store chamber in list of chamber objects
            chambers.Add(chamber);

            //Initialize boss chamber
            if (isBossChamber)
            {
                bossChamber = chamber;
            }
        }
    }

    private void ConnectChambers()
    {
        //Define starting chamber (first room loaded into)
        List<GridPosition> availableChamberPositions = chamberPositions.Keys.ToList();

        //Get random starting point
        int randomStartingChamberIndex = Random.Range(0, chambers.Count);
        Chamber startingChamber = chambers[randomStartingChamberIndex];
        if (startingChamber.IsBossChamber())
        {
            //Get a new starting chamber index (+1 if index is 0. -1 otherwise)
            if (randomStartingChamberIndex == 0)
            {
                randomStartingChamberIndex++;
            }
            else
            {
                randomStartingChamberIndex--;
            }

            startingChamber = chambers[randomStartingChamberIndex];
        }

        Debug.Log($"Starting chamber: {startingChamber.OriginGridPosition}");

        //Pathfind from starting chamber to boss chamber
        ConnectFromStartChamberToBoss(startingChamber, bossChamber);

    }

    private void ConnectFromStartChamberToBoss(Chamber startingChamber, Chamber bossChamber)
    {
        Queue<Chamber> frontier = new Queue<Chamber>();
        frontier.Enqueue(startingChamber);
        int degreesOfSeparationFromBossChamber = 4;

        bool isConnectedToBossChamber = false;
        int loops = 10000;

        while (loops > 0)
        {
            loops--;
            //Get current chamber
            Chamber currentChamber = frontier.Dequeue();

            //Get the 3 closest chambers from this chamber
            Dictionary<Chamber, float> closestChambers = new Dictionary<Chamber, float>();

            //Get 3 closest chambers
            foreach (Chamber chamber in chambers)
            {
                //Ignore if chamber is this chamber
                if (chamber == currentChamber)
                {
                    continue;
                }
                //Ignore if chamber is already connected
                if (chamber.ConnectTo().Contains(currentChamber.OriginGridPosition))
                {
                    continue;
                }

                //calculate distance to from current chamber to the given chamber
                float chamberDistance = currentChamber.OriginGridPosition.Distance(chamber.OriginGridPosition);

                //Check if it can be added
                if (closestChambers.Count < 3)
                {
                    //Not enough chambers to warrant replacing
                    closestChambers.Add(chamber, chamberDistance);
                }
                else
                {
                    //Get the furthest distance from current chamber in the dictionary
                    Chamber farthestChamber = null;
                    float furthestChamberDistance = 0;

                    foreach (KeyValuePair<Chamber, float> chamberPair in closestChambers)
                    {
                        if (chamberPair.Value > furthestChamberDistance)
                        {
                            //Replace furthest chamber with current one
                            farthestChamber = chamberPair.Key;
                            furthestChamberDistance = chamberPair.Value;
                        }
                    }

                    //Test if the current chamber is closer than the furthest chamber
                    if (chamberDistance < furthestChamberDistance)
                    {
                        closestChambers.Remove(farthestChamber);
                        closestChambers.Add(chamber, chamberDistance);
                    }
                }


            }


            //Check if the closest chambers contains the boss chamber
            if (closestChambers.ContainsKey(bossChamber))
            {
                //Make sure that it is separated far enough
                if (degreesOfSeparationFromBossChamber <= 0)
                {
                    //Connect to current chamber and exit
                    currentChamber.AddConnection(bossChamber.OriginGridPosition);
                    isConnectedToBossChamber = true;
                    break;
                }
                else
                {
                    //Remove the boss chamber from connection
                    closestChambers.Remove(bossChamber);
                }
            }

            //Connect to a random other chamber
            List<Chamber> closestChamberKeys = closestChambers.Keys.ToList();
            int randomClosestChamberIndex = Random.Range(0, closestChamberKeys.Count);
            Chamber randomChamber = closestChamberKeys[randomClosestChamberIndex];

            if (randomChamber.OriginGridPosition == new GridPosition(65, 8))
                Debug.Log("Random chamber: " + randomChamber.OriginGridPosition);

            //Add connection to the random chamber
            ConnectChambers(currentChamber, randomChamber);

            //Enqueue random chamber for path exploration
            frontier.Enqueue(randomChamber);
            degreesOfSeparationFromBossChamber--;
        }

        if (debugging)
        {
            if (isConnectedToBossChamber)
            {
                Debug.Log("Connected to boss chamber!");
            }
            else
            {
                Debug.LogWarning("Warning: tried to connect to boss chamber, but couldn't");
            }
        }
    }

    private void ConnectChambers(Chamber currentChamber, Chamber otherChamber)
    {
        currentChamber.AddConnection(otherChamber.OriginGridPosition);
        otherChamber.AddConnection(currentChamber.OriginGridPosition);

        Debug.Log($"Connecting chamber at {currentChamber.OriginGridPosition} to {otherChamber.OriginGridPosition}");

        GridPosition bestHallwayConnectorForCurrentChamber = currentChamber.HallwayConnectors().First().Key;
        GridPosition bestHallwayConnectorForOtherChamber = otherChamber.HallwayConnectors().First().Key;

        float closestHallwayDistance = Mathf.Infinity;

        //Test all available hallway connections.
        foreach(GridPosition currentChamberHallwayGridPosition in currentChamber.HallwayConnectors().Keys)
        {
            //Test for closest distance to other hallway connectors
            foreach (GridPosition otherChamberHallwayGridPosition in otherChamber.HallwayConnectors().Keys)
            {
                //Determine distances
                float connectorDistance = currentChamberHallwayGridPosition.Distance(otherChamberHallwayGridPosition);
                if (connectorDistance < closestHallwayDistance)
                {
                    closestHallwayDistance = connectorDistance;
                    bestHallwayConnectorForCurrentChamber = currentChamberHallwayGridPosition;
                    bestHallwayConnectorForOtherChamber = otherChamberHallwayGridPosition;
                }
            }
        }

        //Store the best chamber position connected to the hallway before using (and removing) the hallway connector at this spot
        Dictionary<GridPosition, GridPosition> hallwayConnectors = currentChamber.HallwayConnectors();
        GridPosition bestHallwayConnector = hallwayConnectors[bestHallwayConnectorForCurrentChamber];

        //Best connectors found. Remove them from respective chambers
        if (!currentChamber.UseHallwayConnector(bestHallwayConnectorForCurrentChamber))
        {
            Debug.LogWarning($"Warning: tried to use hallway connector position for {currentChamber}, but {bestHallwayConnectorForCurrentChamber} is not a valid grid position");
        }
        if (!otherChamber.UseHallwayConnector(bestHallwayConnectorForOtherChamber))
        {
            Debug.LogWarning($"Warning: tried to use hallway connector position for {otherChamber}, but {bestHallwayConnectorForOtherChamber} is not a valid grid position");
        }


        Debug.Log($"Best connectors found. Best hallway connector in current chamber is {bestHallwayConnectorForCurrentChamber} and other chamber is {bestHallwayConnectorForOtherChamber}");

        //Make the connection from current chamber to other chamber
        Queue<GridPosition> frontier = new Queue<GridPosition>();
        frontier.Enqueue(bestHallwayConnectorForCurrentChamber);

        //get the object of the chamber that connects to the hallway

        GridObject chamberConnectionGridObject = map[bestHallwayConnector];

        //Debug.Log($"Best hallway connector for current chamber: {bestHallwayConnectorForCurrentChamber}");
        //Debug.Log($"This connects to the actual chamber at {chamberConnectionGridObject.gridPosition}");

        //Connect the hallway to the chamber
        map[bestHallwayConnectorForCurrentChamber].connectedTo = chamberConnectionGridObject;
        bool connectionCreated = false;

        int numOfLoops = 10000;

        Debug.Log("Connecting the chambers together...");

        while (frontier.Count > 0 && !connectionCreated && numOfLoops > 0)
        {
            numOfLoops--;
            //Pathfind to other chamber
            GridPosition currentPosition = frontier.Dequeue();

            Debug.Log($"Current position: {currentPosition}");

            //Connect adjacent available grid positions to this object
            foreach (GridPosition relativeAdjacentGridPosition in adjacentGridPositions)
            {
                GridPosition absoluteAdjacentGridPosition = currentPosition + relativeAdjacentGridPosition;

                //Check if grid position is within bounds of map
                if (!map.ContainsKey(absoluteAdjacentGridPosition))
                {
                    continue; //out of bounds
                }
                if (map[absoluteAdjacentGridPosition].isExplored)
                {
                    continue;
                }

                //This object was explored
                map[absoluteAdjacentGridPosition].isExplored = true;

                if (map[absoluteAdjacentGridPosition].isHallwayPlaceable)
                {
                    map[absoluteAdjacentGridPosition].connectedTo = map[currentPosition];
                    frontier.Enqueue(absoluteAdjacentGridPosition);
                    Debug.Log($"{absoluteAdjacentGridPosition} is connected to {currentPosition}. Adding that to queue for exploration...");
                    if (currentPosition == bestHallwayConnectorForOtherChamber)
                    {
                        //Connection created!
                        connectionCreated = true;
                        Debug.Log("Connection between chambers created!");
                        break;
                    }
                }
            }
        }

        if (connectionCreated)
        {
            Debug.Log("With the connection created, now creating the actual hallway object");

            GridObject currentObj = map[bestHallwayConnectorForOtherChamber];
            GridPosition currentPosition = bestHallwayConnectorForOtherChamber;

            numOfLoops = 10000;

            while (currentObj.connectedTo != null && numOfLoops > 0)
            {
                Debug.Log($"Current position: {currentPosition}");

                Debug.Log($"current position: {currentObj.gridPosition}");
                Debug.Log($"Connecting to: {currentObj.connectedTo.gridPosition}");

                numOfLoops--;
                //Make the hallways
                Debug.Log("Making hallway at " + currentPosition);
                GameObject hallwayObj = Instantiate(tempHallwayGridObject, GetWorldPositionFromGridPosition(currentPosition), Quaternion.identity);
                hallwayObj.name = $"{currentPosition} Hallway";

                currentObj = currentObj.connectedTo;
                currentPosition = currentObj.gridPosition;
            }

        }
        else
        {
            Debug.LogWarning($"Warning: tried to connect chambers ({currentChamber.OriginGridPosition}, {otherChamber.OriginGridPosition}), but connection was unsuceessfully created");
        }

        //Reset all map exploration states
        foreach(KeyValuePair<GridPosition, GridObject> gridPositionPair in map)
        {
            gridPositionPair.Value.ResetExploration();
        }
    }

    private void PlaceBossChamberOnMap()
    {
        if (debugging)
        {
            Debug.Log("Placing boss chamber on map");
        }

        //Get a random boss chamber
        int randomBossChamber = Random.Range(0, bossRoomLayouts.Length);
        ChamberLayoutSO bossRoomChamberLayout = bossRoomLayouts[randomBossChamber];

        //Get the grid positions for available chamber positions
        List<GridPosition> availableGridPositionsForChambers = availableChamberPositions.Keys.ToList();
        List<GridPosition> availableGridPositions = GetListOfAvailableGridPositionsForChamber(bossRoomChamberLayout, availableGridPositionsForChambers);
        
        //Get random position
        int randomChamberGridPositionIndex = Random.Range(0, availableGridPositions.Count);
        GridPosition randomChamberGridPosition = availableGridPositions[randomChamberGridPositionIndex];

        //Current grid position is no longer being tested for current chamber placement
        availableGridPositions.Remove(randomChamberGridPosition);

        //Try adding the chamber to the map
        if (ChamberCanBePlacedAtPosition(bossRoomChamberLayout, randomChamberGridPosition))
        {
            //Tell the map that the positions that this chamber takes up is no longer available for other chambers
            AddChamberToMap(bossRoomChamberLayout, randomChamberGridPosition, availableGridPositionsForChambers);
        }
        else
        {
            Debug.LogWarning("Warning: tried adding boss room, but failed");
        }
    }

    private void PlaceChambersOnMap()
    {
        if (debugging)
        {
            Debug.Log("Placing chambers on map");
        }

        //Generate a number of chambers to add to the map
        int numOfChambersToPlace = Random.Range(minNumOfChambers, maxNumOfChambers + 1);

        List<GridPosition> availableGridPositionsForChambers = availableChamberPositions.Keys.ToList();

        //Loop through until all chambers are placed, or there are no more positions to place chambers
        while (numOfChambersToPlace > 0 && availableGridPositionsForChambers.Count > 0)
        {
            //if (debugging)
            //{
            //    Debug.Log($"Num of chambers: {numOfChambersToPlace}\navailable chamber grid positions: {availableGridPositionsForChambers.Count}");
            //}

            //Get a random chamber layout to place
            ChamberLayoutSO chamberLayout = GetRandomChamberLayout();

            //if (debugging)
            //{
            //    Debug.Log($"Random chamber layout retrieved: {chamberLayout}");
            //}

            //List of grid positions not yet tested for adding the current chamber
            List<GridPosition> gridPositionsToTest = GetListOfAvailableGridPositionsForChamber(chamberLayout, availableGridPositionsForChambers);
            bool chamberIsNotPlaced = true;

            //Try adding the chamber until out of grid positions to test.
            while (gridPositionsToTest.Count > 0 && chamberIsNotPlaced)
            {
                //if (debugging)
                //{
                //    Debug.Log($"num of grid positions to test: {gridPositionsToTest.Count}\nis the chamber placed: {!chamberIsNotPlaced}");
                //}

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

            GameObject newChamber = Instantiate(chamberPositions[gridPosition].chamberPrefab, chamberWorldPos, Quaternion.identity);
            newChamber.name = $"{gridPosition} Chamber";
        }

        //if (debugging)
        //{
        //    Debug.Log($"Remaining chamber to place: {numOfChambersToPlace}");
        //    Debug.Log($"Remaining chamber positions: {availableGridPositionsForChambers.Count}");


        //    Debug.Log("Map generation complete!");
        //    Debug.Log("Chambers were generated at positions:");
        //    foreach (GridPosition gridPosition in chamberPositions.Keys)
        //    {
        //        Debug.Log(gridPosition);
        //    }
        //}
    }


    /// <summary>
    /// Adds chamber to the map and handles logic for data storage for chamber posiitons. Assumes that the chamber is fully placeable at the given position
    /// </summary>
    /// <param name="chamberLayout"></param>
    /// <param name="originGridPosition"></param>
    private void AddChamberToMap(ChamberLayoutSO chamberLayout, GridPosition originGridPosition, List<GridPosition> availableGridPositionForChambers)
    {
        //if (debugging)
        //{
        //    Debug.Log($"Placing chamber, {chamberLayout}, on map at origin, {originGridPosition}.");
        //}

        //Add the chamber to the dictionary of chamber positions
        chamberPositions[originGridPosition] = chamberLayout;

        //Tell all adjacent chamber positions and the positions of the chamber that they are no longer placeable for other chambers
        List<GridPosition> absoluteChamberGridPositions = GetAbsoluteChamberGridPositions(chamberLayout.GetChamberLayoutGridPositions(), originGridPosition);
        foreach(GridPosition chamberGridPosition in absoluteChamberGridPositions)
        {
            //if (debugging)
            //{
            //    Debug.Log($"Making grid position {chamberGridPosition} part of chamber");
            //    Debug.Log($"Does this chamber exist? {availableGridPositionForChambers.Contains(chamberGridPosition)}");
            //}

            //Current grid position is no longer chamber placeable.
            availableChamberPositions[chamberGridPosition].MakeObjectPartOfChamber();

            availableGridPositionForChambers.Remove(chamberGridPosition);

            //Loop through all adjacent grid positions
            foreach(GridPosition relativeAdjacentGridPosition in adjacentGridPositions)
            {
                GridPosition absoluteAdjacentGridPosition = relativeAdjacentGridPosition + chamberGridPosition;
                //Test if the adjacent position is a chamber position. Skip if it is
                if (absoluteChamberGridPositions.Contains(absoluteAdjacentGridPosition)) { continue; }

                //remove from available grid positions for chambers
                if (availableChamberPositions.TryGetValue(absoluteAdjacentGridPosition, out GridObject gridObject))
                {
                    availableGridPositionForChambers.Remove(absoluteAdjacentGridPosition);
                    availableChamberPositions[absoluteAdjacentGridPosition].SetChamberPlaceable(false);
                }
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
