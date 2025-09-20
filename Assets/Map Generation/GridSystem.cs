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

    List<Chamber> chambersConnectedToStart = new List<Chamber>();

    int mapArea;

    Chamber startingChamber;
    Chamber bossChamber;

    MinEdgePriorityQueue minPQ = new MinEdgePriorityQueue();


    static GridPosition[] adjacentGridPositions =
    {
        new GridPosition(1,0),   //Right
        new GridPosition(0,1),   //Up
        new GridPosition(-1,0),  //Left
        new GridPosition(0,-1),  //Down
    };


    //Debugging times
    float timeSpentPerformingBFS = 0;
    float timeSpentBuildingPaths = 0;
    float timeSpentBuildingChamberEdges = 0;
    float timeSpentTestingIfChambersConnect = 0;
    float timeSpentGettingBestEdgeConnectorsForChambers = 0;
    float timeSpentGettingLengthOfPaths = 0;


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
        float timeToInitializeChamberObjects = Time.realtimeSinceStartup - timeToGenerateChambers;

        ConnectChambers();
        float timeToGenerateConnectors = Time.realtimeSinceStartup - timeToInitializeChamberObjects;

        float timeToGenerateMap = Time.realtimeSinceStartup - timeMapStartedGenerating;

        if (debugging)
        {
            Debug.Log($"Time to generate map: {timeToGenerateMap}s");
            Debug.Log($"Time to generate boss nextChamber: {timeToGenerateBossChamber}");
            Debug.Log($"Time to generate other chambers: {timeToGenerateChambers}");
            Debug.Log($"Time to initialize chambers: {timeToInitializeChamberObjects}");
            Debug.Log($"Time to generate connectors: {timeToGenerateConnectors}");

            Debug.Log("--------------------------------------------------------------------");
            Debug.Log($"Time spent performing BFS: {timeSpentPerformingBFS}");
            Debug.Log($"Time spent building paths: {timeSpentBuildingPaths}");
            Debug.Log($"Time spent building chamber edges: {timeSpentBuildingChamberEdges}");
            Debug.Log($"Time spent testing if chambers can connect: {timeSpentTestingIfChambersConnect}");
            Debug.Log($"Time spent getting best edge connectors for chambers: {timeSpentGettingBestEdgeConnectorsForChambers}");
            Debug.Log($"Time spent getting lengths of paths: {timeSpentGettingLengthOfPaths}");

            if (timeToGenerateMap > 15)
            {
                Debug.LogWarning($"You need to optimize the grid system! {timeToGenerateMap}s is too long a wait for players!!!");
            }
        }
    }

    private void InitializeChamberObjects()
    {
        List<GridPosition> edgeConnectorsToRemove = new List<GridPosition>();

        //Chamber has an origin, hallway connectors, connections to other chambers, and a flag for if it's a boss chamber
        foreach(KeyValuePair<GridPosition, ChamberLayoutSO> chamberPair in chamberPositions)
        {
            //Get the hallway connectors for current chamber pair
            Dictionary<GridPosition, GridPosition> hallwayConnectors = chamberPair.Value.GetAbsoluteHallwayConnectorPositions(chamberPair.Key);
            bool isBossChamber = bossRoomLayouts.Contains(chamberPair.Value);

            //Tell the map that the grid object at the hallway grid positions is connected to a chamebr
            foreach(GridPosition hallwayConnector in hallwayConnectors.Keys.ToList())
            {
                if (map.ContainsKey(hallwayConnector))
                {
                    map[hallwayConnector].isPartOfAChamberHallway = true;
                }
                else
                {
                    //Hallway connector does not exist in the map. REMOVE IT!!!
                    edgeConnectorsToRemove.Add(hallwayConnector);
                }
            }

            //Remove bad edge connectors from chamber
            foreach(GridPosition gridPosition in edgeConnectorsToRemove)
            {
                hallwayConnectors.Remove(gridPosition);
            }

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
        startingChamber = chambers[randomStartingChamberIndex];

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

        BeginConnectingChambers(startingChamber);
    }

    private void BeginConnectingChambers(Chamber startingChamber)
    {
        /*
         * 1) From the starting chamber, make edge connections to EVERY other chamber in the map.
         * 2) Enqueue all of these edges into the minPQ
         * 3) loop through the minPQ until it is out of entries (unlikely), or until every chamber has been visited (very likely)
         * 4) for each dequeue, build the edge connection from chamber A (the chamber already visited) to chamber B (the one not visited yet)
         * 5) After building the connection, tell chamber B to make all of its edges to every unvisited chamber. Queue these edges up for building
         * 6) Make sure to mark chamber B as visited.
         * 7) If an edge connect to chamber B, and chamber B has already been visited, skip this edge. No looping allowed yet
         * 8) Be sure to use the chamber's connection when building the edge. Also be sure to check if the edge connector exists before building the edge. If it doesn't exist, then skip it.
         */

        //Clear minPQ just in case
        minPQ.Clear();

        //1) make edge connections to other chambers from starting chamber:
        //2) Handled within the method
        CreateAndQueueEdgesForChamber(startingChamber);

        //starting chamber visited by default
        startingChamber.SetIsVisited(true);

        //3) Loop through minPQ until out of eges or all chambers visited
        int loops = 100000; //failsafe
        while (minPQ.Count() > 0 && !AllChambersVisited() && loops > 0)
        {
            loops--;

            //Get current edge
            Edge currentEdge = minPQ.Dequeue();

            //Get chambers in the edge
            Chamber chamberA = currentEdge.GetChamberA();
            Chamber chamberB = currentEdge.GetChamberB();

            //If chamber B (the supposed unvisited one) has already been visited, then skip this edge. No looping allowed yet
            if (chamberB.IsVisited()) continue;

            //Chamber have been visited
            chamberA.SetIsVisited(true); //Should be redundant
            chamberB.SetIsVisited(true);

            //4) Build edge path between chamber A's and B's edge connectors
            List<GridPosition> path = PerformBFS(currentEdge.GetEdgeConnectorForChamberA(), currentEdge.GetEdgeConnectorForChamberB());
            BuildPath(path);

            //Tell chamber A and B their respective edge connector was consumed
            chamberA.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberA());
            chamberB.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberB());

            //5) Tell chamber B to make and add its edges to every unvisited chamber.
            CreateAndQueueEdgesForChamber(chamberB);
        }
        if (loops <= 0)
        {
            Debug.LogError("Error: Tried to connect chambers, but looped too many times");
        }
    }

    private bool AllChambersVisited()
    {
        //Loop through all chambers. If a single one has not been visited, return false
        foreach(Chamber chamber in chambers)
        {
            if (!chamber.IsVisited()) return false;
        }

        return true;
    }

    private void BuildPath(List<GridPosition> path)
    {
        float timeStartBuildingPath = Time.realtimeSinceStartup;

        //TODO: update this to handle path corners, and replace temp prefab with actual hallway prefabs

        //Loop through each path position
        foreach(GridPosition gridPosition in path)
        {
            GameObject hallwayObj = Instantiate(tempHallwayGridObject, GetWorldPositionFromGridPosition(gridPosition), Quaternion.identity);
            hallwayObj.name = $"{gridPosition} Hallway";

            //update the map with the new hallway objects
            map[gridPosition].MakeObjectAHallway();
        }

        float timeEndBuildingPaths = Time.realtimeSinceStartup;
        timeSpentBuildingPaths += timeEndBuildingPaths - timeStartBuildingPath;
    }

    private void CreateAndQueueEdgesForChamber(Chamber givenChamber)
    {
        float timeStartBuildingEdges = Time.realtimeSinceStartup;

        //Loop through all chambers on the map, exclude given chamber & chamber that cannot be connected to
        foreach(Chamber chamber in chambers)
        {
            if (chamber == givenChamber) continue;
            if (!CanConnectChambers(givenChamber, chamber)) continue;
            if (chamber.IsVisited()) continue;

            //Chambers can be connected. Get the best edge connectors for each chamber and its edge cost
            (GridPosition bestChamberAEdgeConnector, GridPosition bestChamberBEdgeConnector) = GetBestEdgeConnectors(givenChamber, chamber);

            List<GridPosition> path = PerformBFS(bestChamberAEdgeConnector, bestChamberBEdgeConnector);
            int edgeCost = GetPathLength(path);

            //Build the edge
            Edge edge = new Edge(
                path,
                bestChamberAEdgeConnector,
                bestChamberBEdgeConnector,
                givenChamber,
                chamber,
                edgeCost
                );
            //Queue edge for exploration
            minPQ.Enqueue(edge);
        }

        float timeEndBuildingEdges = Time.realtimeSinceStartup;
        timeSpentBuildingChamberEdges += timeEndBuildingEdges - timeStartBuildingEdges;
    }

    bool CanConnectChambers(Chamber chamberA, Chamber chamberB)
    {
        float timeStartTestingIfChambersConnect = Time.realtimeSinceStartup;
        float timeEndTestingIfChambersConnect;

        bool canConnectChambers = false;

        //Get best hallway connectors for chamber connection
        (GridPosition bestHallwayConnectorForStartChamber, GridPosition bestHallwayConnectorForOtherChamber) = GetBestEdgeConnectors(chamberA, chamberB);

        if (bestHallwayConnectorForStartChamber == GridPosition.Invalid || bestHallwayConnectorForOtherChamber == GridPosition.Invalid)
        {
            timeEndTestingIfChambersConnect = Time.realtimeSinceStartup;
            timeSpentTestingIfChambersConnect += timeEndTestingIfChambersConnect - timeStartTestingIfChambersConnect;

            //Connection does not exist
            return false;
        }

        List<GridPosition> path = PerformBFS(bestHallwayConnectorForStartChamber, bestHallwayConnectorForOtherChamber);
        if (path == null)
        {
            //No path exists
            canConnectChambers = false;
        }
        else
        {
            //Path exists
            canConnectChambers = true;
        }

        timeEndTestingIfChambersConnect = Time.realtimeSinceStartup;
        timeSpentTestingIfChambersConnect += timeEndTestingIfChambersConnect - timeStartTestingIfChambersConnect;

        return canConnectChambers;
    }

    (GridPosition bestHallwayConnectorForStartChamber, GridPosition bestHallwayConnectorForOtherChamber) GetBestEdgeConnectors(
        Chamber startChamber, 
        Chamber otherChamber
        )
    {
        float timeStartGettingEdgeConnectors = Time.realtimeSinceStartup;

        //If there are no hallway connectors for either chamber, then return null objects
        if (startChamber.HallwayConnectors().Count <= 0 || otherChamber.HallwayConnectors().Count <= 0)
        {
            return (GridPosition.Invalid, GridPosition.Invalid);
        }

        //Get best hallway connectors for chamber connection
        GridPosition bestHallwayConnectorForStartChamber = startChamber.HallwayConnectors().Keys.First();
        GridPosition bestHallwayConnectorForOtherChamber = otherChamber.HallwayConnectors().Keys.First();

        int shortestDistanceForHallwayConnectors = int.MaxValue;

        //Determine the shortest path between hallway connectors
        foreach (GridPosition startHallwayConnectorPosition in startChamber.HallwayConnectors().Keys.ToList())
        {
            //If the hallway connector is outside the map, then skip it
            if (!map.ContainsKey(startHallwayConnectorPosition)) { continue; }

            foreach (GridPosition otherHallwayConnectorPosition in otherChamber.HallwayConnectors().Keys.ToList())
            {
                //If the hallway connector is outside the map, then skip it
                if (!map.ContainsKey(otherHallwayConnectorPosition)) { continue; }

                int pathLength = GetPathLength(startHallwayConnectorPosition, otherHallwayConnectorPosition);
                if (pathLength < shortestDistanceForHallwayConnectors)
                {
                    bestHallwayConnectorForStartChamber = startHallwayConnectorPosition;
                    bestHallwayConnectorForOtherChamber = otherHallwayConnectorPosition;
                    shortestDistanceForHallwayConnectors = pathLength;
                }
            }
        }

        float timeEndGettingEdgeConnectors = Time.realtimeSinceStartup;
        timeSpentGettingBestEdgeConnectorsForChambers += timeEndGettingEdgeConnectors - timeStartGettingEdgeConnectors;

        return (bestHallwayConnectorForStartChamber, bestHallwayConnectorForOtherChamber);
    }

    private int GetPathLength(GridPosition chamberAEdgeConnector, GridPosition chamberBEdgeConnector)
    {
        float timeStartGettingPathLength = Time.realtimeSinceStartup;

        List<GridPosition> path = PerformBFS(chamberAEdgeConnector, chamberBEdgeConnector);

        float timeEndGettignPathLength = Time.realtimeSinceStartup;
        timeSpentGettingLengthOfPaths += timeEndGettignPathLength - timeStartGettingPathLength;

        return GetPathLength(path);
    }

    int GetPathLength(List<GridPosition> path)
    {
        float timeStartGettingPathLength = Time.realtimeSinceStartup;
        float timeEndGettignPathLength;

        //Check if path was created
        if (path == null)
        {
            timeEndGettignPathLength = Time.realtimeSinceStartup;
            timeSpentGettingLengthOfPaths += timeEndGettignPathLength - timeStartGettingPathLength;

            return int.MaxValue;
        }

        timeEndGettignPathLength = Time.realtimeSinceStartup;
        timeSpentGettingLengthOfPaths += timeEndGettignPathLength - timeStartGettingPathLength;

        //Get path length
        return path.Count;
    }

    List<GridPosition> PerformBFS(GridPosition chamberAEdgeConnector, GridPosition chamberBEdgeConnector)
    {
        float timeStartedBFS = Time.realtimeSinceStartup;

        List<GridPosition> path = new List<GridPosition>();

        //Reset pathfinding exploration in the map
        foreach(GridObject gridObject in map.Values.ToList())
        {
            gridObject.isExplored = false;
            gridObject.connectedTo = null;
        }

        //Perform BFS
        Queue<GridPosition> frontier = new Queue<GridPosition>();
        map[chamberAEdgeConnector].isExplored = true;
        frontier.Enqueue(chamberAEdgeConnector);

        //If the starting position is the ending position, then there is no need to continue pathfinding
        if (chamberAEdgeConnector == chamberBEdgeConnector)
        {
            map[chamberBEdgeConnector].connectedTo = map[chamberAEdgeConnector];
            path.Add(chamberAEdgeConnector);
            return path;
        }

        GridPosition currentPos = chamberAEdgeConnector;

        bool pathWasCreated = false;
        int loops = 10000; //failsafe
        while (frontier.Count > 0 && loops > 0)
        {
            loops--;
            currentPos = frontier.Dequeue();

            //If the current position is the end, then pathfinding is complete
            if (currentPos == chamberBEdgeConnector)
            {
                //Path created
                pathWasCreated = true;
                break;
            }

            //Explore neighbors
            foreach (GridPosition relativeNeighborPos in adjacentGridPositions)
            {
                GridPosition absoluteNeighborGridPos = relativeNeighborPos + currentPos;

                //Conditions to ignore position
                if (!map.ContainsKey(absoluteNeighborGridPos))
                {
                    continue;
                }
                if (map[absoluteNeighborGridPos].isExplored)
                {
                    continue;
                }
                if (!map[absoluteNeighborGridPos].isHallwayPlaceable)
                {
                    continue;
                }
                if (map[absoluteNeighborGridPos].isPartOfAChamberHallway && absoluteNeighborGridPos != chamberBEdgeConnector)
                {
                    continue;
                }

                //Neighbor position explored
                map[absoluteNeighborGridPos].isExplored = true;
                //Connect to current pos
                map[absoluteNeighborGridPos].connectedTo = map[currentPos];

                //Queue for exploration
                frontier.Enqueue(absoluteNeighborGridPos);
            }
        }

        if (loops <= 0)
        {
            Debug.LogError("Error: Too many loops while performing BFS!");
        }

        if (!pathWasCreated)
        {
            //Path does not exist
            return null;
        }

        //Build the path
        while (currentPos != chamberAEdgeConnector)
        {
            path.Add(currentPos);
            currentPos = map[currentPos].connectedTo.gridPosition;
        }

        //Add the starting edge connector (not included in looping process)
        path.Add(chamberAEdgeConnector);
        //Reverse order from chamber A to chamber B
        path.Reverse();

        float timeEndedBFS = Time.realtimeSinceStartup;
        timeSpentPerformingBFS += timeEndedBFS - timeStartedBFS;

        return path;
    }

    private void PlaceBossChamberOnMap()
    {
        if (debugging)
        {
            Debug.Log("Placing boss nextChamber on map");
        }

        //Get a random boss chamber
        int randomBossChamber = Random.Range(0, bossRoomLayouts.Length);
        ChamberLayoutSO bossRoomChamberLayout = bossRoomLayouts[randomBossChamber];

        //Get the grid positions for available chamber positions
        List<GridPosition> availableGridPositionsForChamber = availableChamberPositions.Keys.ToList();
        List<GridPosition> availableGridPositionsForBossChamber = GetListOfAvailableGridPositionsForChamber(bossRoomChamberLayout, availableGridPositionsForChamber);
        
        //Get random position
        int randomChamberGridPositionIndex = Random.Range(0, availableGridPositionsForBossChamber.Count);
        GridPosition randomChamberGridPosition = availableGridPositionsForBossChamber[randomChamberGridPositionIndex];

        //Current grid position is no longer being tested for current chamber placement
        availableGridPositionsForBossChamber.Remove(randomChamberGridPosition);

        //Try adding the chamber to the map
        if (ChamberCanBePlacedAtPosition(bossRoomChamberLayout, randomChamberGridPosition))
        {
            //Tell the map that the positions that this chamber takes up is no longer available for other chambers
            AddChamberToMap(bossRoomChamberLayout, randomChamberGridPosition, availableGridPositionsForChamber);
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
            //Get a random chamber layout to place
            ChamberLayoutSO chamberLayout = GetRandomChamberLayout();

            //List of grid positions not yet tested for adding the current chamber
            List<GridPosition> gridPositionsToTest = GetListOfAvailableGridPositionsForChamber(chamberLayout, availableGridPositionsForChambers);
            bool chamberIsNotPlaced = true;

            //Try adding the chamber until out of grid positions to test.
            while (gridPositionsToTest.Count > 0 && chamberIsNotPlaced)
            {
                //Get random grid position for the origin of the chamber
                int randomChamberOriginPositionIndex = Random.Range(0, gridPositionsToTest.Count);
                GridPosition originChamberNodeGridPosition = gridPositionsToTest[randomChamberOriginPositionIndex];

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
                Debug.LogWarning("Warning: Tried adding nextChamber to map, but the nextChamber could not be placed!");
            }

            numOfChambersToPlace--;
        }

        //Place the chambers at the grid positions
        foreach(KeyValuePair<GridPosition, ChamberLayoutSO> chamberKeyValuePair in chamberPositions)
        {
            GridPosition gridPosition = chamberKeyValuePair.Key;
            Vector3 chamberWorldPos = GetWorldPositionFromGridPosition(gridPosition);

            GameObject newChamber = Instantiate(chamberKeyValuePair.Value.chamberPrefab, chamberWorldPos, Quaternion.identity);
            newChamber.name = $"{gridPosition} Chamber";
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
            map[chamberGridPosition].MakeObjectPartOfChamber();

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
            else if (gridPosition.z > maxZIndexForChamber)
            {
                //Not enough height for chamber
                gridPositionsToRemove.Add(gridPosition);
                continue;
            }
            else if (map.ContainsKey(gridPosition))
            {
                if (!map[gridPosition].isChamberPlaceable)
                {
                    //Cannot place chamber here
                    gridPositionsToRemove.Add(gridPosition);
                }
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
            Debug.LogWarning("Warning: nextChamber layouts was not initialized!");
            return null;
        }
        if (chamberLayouts.Length == 0)
        {
            Debug.LogWarning("Warning: no nextChamber layouts given to create map!");
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
            GridPosition chamberNodeGridPosition = chamberNode.relativeChamberNodeGridPosition + originGridPositionOnMap;

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

            if (!map[gridPositionOnMap].isChamberPlaceable)
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
