using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public enum PathfindingAlgorithm
    {
        AStar,
        BreadthFirstSearch,
        JumpPointSearch
    }

    [Header("Player")]
    [SerializeField]
    GameObject playerPrefab;

    [Header("Chamber References")]
    [SerializeField]
    ChamberLayoutSO[] chamberLayouts;

    [SerializeField]
    ChamberLayoutSO[] bossRoomLayouts;


    [Header("Chamber Type Settings")]
    [SerializeField, Range(0, 1), Tooltip("The chance for the chamber to be a shop chamber. Note: Ensure all types of chamber chances add to 1 for ease of debugging")]
    float chanceForChamberToBeShop = 0.1f;

    [SerializeField, Tooltip("The items that any given chamber may contain as a reward")]
    Item[] itemRewards;


    [Header("Managers")]
    [SerializeField] 
    EnemySpawnManager spawnManager; // assign via Inspector


    [Header("Hallway References")]
    [SerializeField]
    GameObject hallwayForwardPlaceholder;

    [SerializeField]
    GameObject hallwayRightwardPlaceholder;

    [SerializeField]
    GameObject hallwayLeftForwardPlaceholder;

    [SerializeField]
    GameObject hallwayRightForwardPlaceholder;

    [SerializeField]
    GameObject hallwayLeftBackwardPlaceholder;

    [SerializeField]
    GameObject hallwayRightBackwardPlaceholder;



    [Header("Map Settings")]
    [SerializeField, Min(1), Tooltip("The lenght of the map")]
    int mapLength = 10;

    [SerializeField, Min(1), Tooltip("The gridWidth of the map")]
    int mapWidth = 10;

    [SerializeField, Min(1), Tooltip("The minimum number of chambers that will be placed on the map")]
    int minNumOfChambers = 5;

    [SerializeField, Min(1), Tooltip("The maximum number of chambers that will be placed on the map")]
    int maxNumOfChambers = 100;

    [SerializeField, Min(0), Tooltip("After connecting all the chambers, additional connections will be made. This controls how long those additional connections may be")]
    int maxPathLengthForAdditionalConnection = 15;

    [SerializeField, Range(0, 1), Tooltip("The base chance for an additional chamber connection to be generated")]
    float chanceForAdditionalConnection = 0.15f;

    [SerializeField, Min(0), Tooltip("After connecting all the chambers, additional connections will be made. This controls the maximum number of additional connections that will be generated on the map")]
    int maxNumOfExtraConnections = 20;

    [SerializeField, Range(100, 10000), Tooltip("After connecting all the chambers, additional connections will be made. This controls how many connection tests will be performed before the algorithm stops")]
    int numOfAdditionalConnectionsToTest = 500;

    [SerializeField, Min(1), Tooltip("The scale of the actual objects when placing them in the world. In the Unity editor, reference the size of the small grid. That is the base unit of measurement")]
    float mapScale = 10f;


    [Header("Pathfinding Settings")]
    [SerializeField, Tooltip("The type of pathfinding algorithm that will be used to generate connections. Note: performance may be impacted by using less efficient pathfinding algorithms")]
    PathfindingAlgorithm pathfindingAlgorithmToUse = PathfindingAlgorithm.AStar;

    [SerializeField, Tooltip("Controls whether debug messages should be displayed")]
    bool debugging;


    [Header("Camera Settings")]
    [SerializeField]
    Camera mapCamera;



    //Map Data Structures
    GridObject[,] map;
    Dictionary<GridPosition, ChamberLayoutSO> chamberPositions = new Dictionary<GridPosition, ChamberLayoutSO>();
    Dictionary<GridPosition, GameObject> chamberGameObjects = new Dictionary<GridPosition, GameObject>();
    Dictionary<GridPosition, GridObject> availableChamberPositions = new Dictionary<GridPosition, GridObject>();
    List<Chamber> chambers = new List<Chamber>();
    List<Edge> edges = new List<Edge>();
    Chamber startingChamber;
    Chamber bossChamber;

    //Pathfinding Properties
    MinPriorityQueue<Edge> minPQ = new MinPriorityQueue<Edge>();
    int lastVisitID = 0;
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
    float timeSpentPerformingAStar = 0;

    private void Awake()
    {
        float timeMapStartedGenerating = Time.realtimeSinceStartup;

        //Initialize the map
        map = new GridObject[mapLength, mapWidth];
        for (int i = 0; i < mapLength; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                //Make the grid position
                GridPosition gridPos = new GridPosition(i, j);

                //Make grid opbject at map position
                map[i, j] = new GridObject(gridPos);

                //Initialize chamber positions
                availableChamberPositions.Add(gridPos, map[gridPos.x, gridPos.z]);
            }
        }

        PlaceBossChamberOnMap();
        float timeToGenerateBossChamber = Time.realtimeSinceStartup - timeMapStartedGenerating;

        PlaceChambersOnMap();
        float timeToGenerateChambers = Time.realtimeSinceStartup - timeToGenerateBossChamber;

        InitializeChamberObjects();
        float timeToInitializeChamberObjects = Time.realtimeSinceStartup - timeToGenerateChambers;

        ConnectChambers();
        float timeToGenerateConnectors = Time.realtimeSinceStartup - timeToInitializeChamberObjects;

        GenerateAdditionalChamberEdges();
        float timeSpentGeneratingAdditionalChamberConnections = Time.realtimeSinceStartup - timeToGenerateConnectors;

        float timeToGenerateMap = Time.realtimeSinceStartup - timeMapStartedGenerating;

        if (debugging)
        {
            Debug.Log($"Time to generate map: {timeToGenerateMap}s");
            Debug.Log($"Time to generate boss nextChamber: {timeToGenerateBossChamber}");
            Debug.Log($"Time to generate other chambers: {timeToGenerateChambers}");
            Debug.Log($"Time to initialize chambers: {timeToInitializeChamberObjects}");
            Debug.Log($"Time to generate connectors: {timeToGenerateConnectors}");
            Debug.Log($"Time to generate additional connectors: {timeSpentGeneratingAdditionalChamberConnections}");

            Debug.Log("--------------------------------------------------------------------");
            Debug.Log($"Time spent performing BFS: {timeSpentPerformingBFS}");
            Debug.Log($"Time spent performing A*: {timeSpentPerformingAStar}");
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

        //Generate player into the map
        //Debug.Log("player starting grid position: " + startingChamber.GetPlayerSpawnPosition());
        //Debug.Log("player starting grid position: " + startingChamber.GetPlayerSpawnPosition());
        GameObject playerObj = Instantiate(playerPrefab, GetPlayerSpawnPosition(startingChamber) + new Vector3(5, 3, 5), Quaternion.identity);
    }
    private void Start()
    {
        InitializeChamberMonobehaviours();
        InitializeEdgeMonobehaviours();
        SetupCameraSettings();
    }

    private void SetupCameraSettings()
    {
        /*
         * 1) To setup the camera, it needs to capture the entire map. This can be done by centralizing the camera to the center of the map
         *    The center of the map can be a little strange. Say it has an even length, then the center of the map is in between the two
         *    middle grid positions. When it's odd then the middle grid position is the camera's position. Thus, you need to determine
         *    if the dimensions are even or odd before determining the center.
         * 2) After getting the center, the camera's height & orthographic size needs to be adjusted to fit the entire map. Considerations for the near & far
         *    clipping plane also should be taken.
         */

        //Get center of the map
        int centerWidth = mapWidth / 2;
        int centerLength = mapLength / 2;

        //Get the gridposition for the center
        GridPosition centerGridPosition = new GridPosition(centerLength, centerWidth);

        //Get world position from this position
        Vector3 centerWorldPos = GetWorldPositionFromGridPosition(centerGridPosition);

        //If length/width are even, add world unit measurement
        Vector3 unitGridVectorPosition = GetWorldPositionFromGridPosition(GridPosition.One) / 2;
        if (centerWidth % 2 == 0)
        {
            centerWorldPos += new Vector3(0,0,unitGridVectorPosition.z);
        }
        if (centerLength % 2 == 0)
        {
            centerWorldPos += new Vector3(unitGridVectorPosition.x, 0, 0);
        }

        //set camera height
        float cameraHeight = 50;
        centerWorldPos += new Vector3(0, cameraHeight, 0);

        //Set camera center position
        mapCamera.transform.SetPositionAndRotation(centerWorldPos, mapCamera.transform.rotation);
        //Debug.Log("Camera center position: " + centerWorldPos);


        //Set the orthographic size of the camera to fit the entire map.
        mapCamera.orthographicSize = Mathf.Max(
            centerWorldPos.z,
            centerWorldPos.x / mapCamera.aspect
        );

        //Set camera height, near clip plane, & far clip plane
        mapCamera.nearClipPlane = 0.0001f;
        mapCamera.farClipPlane = mapCamera.transform.position.y + 1;
    }

    private void InitializeEdgeMonobehaviours()
    {
        foreach (Edge edge in edges) //I'm on the edge... of glory
        {
            Chamber chamberA = edge.GetChamberA();
            Chamber chamberB = edge.GetChamberB();

            chamberA.AddEdgeMonobehaviour(edge.GetEdgeMonobehaviour());
            chamberB.AddEdgeMonobehaviour(edge.GetEdgeMonobehaviour());
        }
    }

    private void InitializeChamberMonobehaviours()
    {
        if (spawnManager == null)
        {
            Debug.LogError("EnemySpawnManager reference is missing!");
        }

        // Build a dictionary for fast lookups
        Dictionary<GridPosition, Chamber> chamberLookup = new Dictionary<GridPosition, Chamber>();
        foreach (Chamber chamber in chambers)
        {
            chamberLookup[chamber.OriginGridPosition()] = chamber;
        }

        foreach (KeyValuePair<GridPosition, GameObject> chamberPositionObjectValuePair in chamberGameObjects)
        {
            GameObject chamberObj = chamberPositionObjectValuePair.Value;
            GridPosition chamberPos = chamberPositionObjectValuePair.Key;
            ChamberMonoBehaviour chamberMonoBehaviour = chamberObj.GetComponent<ChamberMonoBehaviour>();

            if (!chamberLookup.TryGetValue(chamberPos, out Chamber chamber))
            {
                Debug.LogError($"Error: Chamber at {chamberPos} does not exist, but monobehaviour object needs one!");
                continue;
            }

            chamberMonoBehaviour.Initialize(chamber);

            // Register with EnemySpawnManager directly — no search required
            if (spawnManager != null)
            {
                spawnManager.RegisterChamber(chamberMonoBehaviour);
            }
        }

        // Initialize starting chamber
        startingChamber.GetMonobehaviour().HandlePlayerEnteredChamber();
    }

    private Vector3 GetPlayerSpawnPosition(Chamber startingChamber)
    {
        GridPosition playerSpawnPos = startingChamber.GetPlayerSpawnPosition();

        if (playerSpawnPos == GridPosition.Invalid)
        {
            Debug.LogError($"Error: {startingChamber.GetChamberLayoutSO()} does not define a starting position!");
        }

        //Get the starting spawn position from the chamber
        return GetWorldPositionFromGridPosition(playerSpawnPos);
    }

    //Methods for Chamber Generation
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
        List<GridPosition> availableGridPositionsForBossChamber = GetListOfAvailableChamberGridPositions(bossRoomChamberLayout, availableGridPositionsForChamber);

        //Get random position
        int randomChamberGridPositionIndex = Random.Range(0, availableGridPositionsForBossChamber.Count);
        GridPosition randomChamberGridPosition = availableGridPositionsForBossChamber[randomChamberGridPositionIndex];

        //Current grid position is no longer being tested for current chamber placement
        availableGridPositionsForBossChamber.Remove(randomChamberGridPosition);

        //Try adding the chamber to the map
        if (CanPlaceChamberAtPosition(bossRoomChamberLayout, randomChamberGridPosition))
        {
            //Tell the map that the positions that this chamber takes up is no longer available for other chambers
            AddChamberToMap(bossRoomChamberLayout, randomChamberGridPosition, availableGridPositionsForChamber);
        }
        else if (debugging)
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
            List<GridPosition> gridPositionsToTest = GetListOfAvailableChamberGridPositions(chamberLayout, availableGridPositionsForChambers);
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
                if (CanPlaceChamberAtPosition(chamberLayout, originChamberNodeGridPosition))
                {
                    //Chamber can be added to map
                    chamberIsNotPlaced = false;

                    //Tell the map that the positions that this chamber takes up is no longer available for other chambers
                    AddChamberToMap(chamberLayout, originChamberNodeGridPosition, availableGridPositionsForChambers);
                }
            }

            if (chamberIsNotPlaced && debugging)
            {
                Debug.LogWarning("Warning: Tried adding nextChamber to map, but the nextChamber could not be placed!");
            }

            numOfChambersToPlace--;
        }

        //Place the chambers at the grid positions
        foreach (KeyValuePair<GridPosition, ChamberLayoutSO> chamberKeyValuePair in chamberPositions)
        {
            GridPosition gridPosition = chamberKeyValuePair.Key;
            Vector3 chamberWorldPos = GetWorldPositionFromGridPosition(gridPosition);

            GameObject newChamber = Instantiate(chamberKeyValuePair.Value.chamberPrefab, chamberWorldPos, Quaternion.identity);
            chamberGameObjects.Add(gridPosition, newChamber);
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
        foreach (GridPosition chamberGridPosition in absoluteChamberGridPositions)
        {
            //Current grid position is no longer chamber placeable.
            availableChamberPositions[chamberGridPosition].MakeObjectPartOfChamber();
            map[chamberGridPosition.x, chamberGridPosition.z].MakeObjectPartOfChamber();

            availableGridPositionForChambers.Remove(chamberGridPosition);

            //Loop through all adjacent grid positions
            foreach (GridPosition relativeAdjacentGridPosition in adjacentGridPositions)
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
    private List<GridPosition> GetListOfAvailableChamberGridPositions(ChamberLayoutSO chamberLayout, List<GridPosition> availableGridPositionsOnMap)
    {
        //TODO: Optimize this method! Many redundant loops

        //Grid positions to return
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

        foreach (GridPosition gridPosition in availableGridPositions)
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
            else if (IsValidMapPosition(gridPosition))
            {
                if (!map[gridPosition.x, gridPosition.z].isChamberPlaceable)
                {
                    //Cannot place chamber here
                    gridPositionsToRemove.Add(gridPosition);
                }
            }
        }

        foreach (GridPosition gridPosition in gridPositionsToRemove)
        {
            availableGridPositions.Remove(gridPosition);
        }

        return availableGridPositions;
    }
    ChamberLayoutSO GetRandomChamberLayout()
    {
        if (chamberLayouts == null)
        {
            Debug.LogError("Warning: nextChamber layouts was not initialized!");
            return null;
        }
        if (chamberLayouts.Length == 0)
        {
            Debug.LogError("Warning: no nextChamber layouts given to create map!");
            return null;
        }

        int numOfChamberLayouts = chamberLayouts.Length;
        int randomChamberIndex = Random.Range(0, numOfChamberLayouts);
        return chamberLayouts[randomChamberIndex];
    }
    /// <summary>
    /// Tests to see if the chamber can be placed at the given grid position for the chamber's origin position
    /// </summary>
    /// <param name="chamberLayout">The scriptable object for the chamber</param>
    /// <param name="originGridPosition">The origin of the chamber on the map</param>
    /// <returns>True if chamber fits at the given position. False otherwise.</returns>
    bool CanPlaceChamberAtPosition(ChamberLayoutSO chamberLayout, GridPosition originGridPosition)
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

            if (!map[gridPositionOnMap.x, gridPositionOnMap.z].isChamberPlaceable)
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
    private void InitializeChamberObjects()
    {
        List<GridPosition> edgeConnectorsToRemove = new List<GridPosition>();

        //Chamber has an origin, hallway connectors, connections to other chambers, and a flag for if it's a boss chamber
        foreach (KeyValuePair<GridPosition, ChamberLayoutSO> chamberPair in chamberPositions)
        {
            //Get the hallway connectors for current chamber pair
            Dictionary<GridPosition, GridPosition> hallwayConnectors = chamberPair.Value.GetAbsoluteHallwayConnectorPositions(chamberPair.Key);
            bool isBossChamber = bossRoomLayouts.Contains(chamberPair.Value);

            //Tell the map that the grid object at the hallway grid positions is connected to a chamebr
            foreach (GridPosition hallwayConnector in hallwayConnectors.Keys.ToList())
            {
                if (IsValidMapPosition(hallwayConnector))
                {
                    map[hallwayConnector.x, hallwayConnector.z].isPartOfAChamberHallway = true;
                }
                else
                {
                    //Hallway connector does not exist in the map. REMOVE IT!!!
                    edgeConnectorsToRemove.Add(hallwayConnector);
                }
            }

            //Remove bad edge connectors from chamber
            foreach (GridPosition gridPosition in edgeConnectorsToRemove)
            {
                hallwayConnectors.Remove(gridPosition);
            }

            //Determine what type of chamber the chamber will be. Default to item reward
            Chamber.ChamberType chamberType = Chamber.ChamberType.ItemReward;
            if (isBossChamber)
            {
                chamberType = Chamber.ChamberType.Boss;
            }
            else
            {
                //Roll the chance for the chamber being a shop or item
                chamberType = RollForChamberType();
            }

            //Determine what type of item the chamber will contain
            Item itemReward = null;

            if (chamberType == Chamber.ChamberType.ItemReward)
            {
                itemReward = RollForItemReward();
            }

            //Generate the chamber object
            Chamber chamber = new Chamber(
                chamberPair.Key, 
                chamberPair.Value, 
                hallwayConnectors, 
                isBossChamber, 
                chamberType,
                itemReward
                );

            //Store chamber in list of chamber objects
            chambers.Add(chamber);

            //Initialize boss chamber
            if (isBossChamber)
            {
                bossChamber = chamber;
            }
        }
    }

    private Item RollForItemReward()
    {
        if (itemRewards.Length == 0)
        {
            Debug.LogWarning($"Warning: {gameObject} does not contain any item rewards!");
            return null;
        }

        //Default the item reward to first
        Item itemReward = itemRewards[0];

        //if only one item, return it
        if (itemRewards.Length == 1)
        {
            return itemReward;
        }
        else
        {
            //Get a random item
            int randomItemRewardIndex = Random.Range(0, itemRewards.Length);
            itemReward = itemRewards[randomItemRewardIndex];
        }

       return itemReward;
    }

    /// <summary>
    /// Determines what type of chamber you have based on the chances given in the inspector
    /// </summary>
    /// <returns>The type of chamber. Defaults to ItemReward if no other chamber reward was rolled</returns>
    private Chamber.ChamberType RollForChamberType()
    {
        //Create a random number between 0 and 1
        float randomChance = Random.Range(0, 1f);

        //Check if this will be a shop chamber type
        if (randomChance > 0 && randomChance <= chanceForChamberToBeShop)
        {
            return Chamber.ChamberType.Shop;
        }

        //No chamber type given, default to item reward
        return Chamber.ChamberType.ItemReward;
    }

    //Methods for chamber edge generation
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
        int loops = 1000000; //failsafe
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
            List<GridPosition> path = UsePathfindingAlgorithm(
                currentEdge.GetEdgeConnectorForChamberA(),
                currentEdge.GetEdgeConnectorForChamberB()
                );

            if (path == null)
            {
                //path does not exist. Do not build path
                chamberB.SetIsVisited(false);
                continue;
            }
            BuildEdge(currentEdge, path);

            //Tell chamber A and B their respective edge connector was consumed
            chamberA.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberA());
            chamberB.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberB());

            //Tell chamber A and B they are connected to one another
            chamberA.AddConnection(chamberB);
            chamberB.AddConnection(chamberA);

            //If chamber B is boss chamber, do not add it to queue (only 1 connection to boss chamber rule)
            if (chamberB.IsBossChamber())
            {
                continue;
            }

            //5) Tell chamber B to make and add its edges to every unvisited chamber.
            CreateAndQueueEdgesForChamber(chamberB);
        }
        if (loops <= 0)
        {
            Debug.LogError("Error: Tried to connect chambers, but looped too many times");
        }
    }
    private void GenerateAdditionalChamberEdges()
    {
        /*
         * When to stop generating extra chamber connections:
         * 
         * 1) If there are physically no more edges to connect (minPQ runs out)
         * 2) If # of extra connections allowed has been exceeded
         */

        //Reuse the min PQ from the first connection phase to generate chambers
        int extraConnectionsGenerated = 0;
        while (minPQ.Count() > 0 && extraConnectionsGenerated < maxNumOfExtraConnections && numOfAdditionalConnectionsToTest > 0)
        {
            /*
             * Conditions to skip the extra edge connection
             *  1) If the chance does not favor the edge connection
             *  2) If the length of the edge exceeds the maximum allowed for additional edges
             *  3) If the chamber cannot physcially handle more connections (already handled)
             */

            //Get current edge
            Edge currentEdge = minPQ.Dequeue();

            //Get chambers in the edge
            Chamber chamberA = currentEdge.GetChamberA();
            Chamber chamberB = currentEdge.GetChamberB();

            //1) roll for edge generation
            if (Random.Range(0f, 1f) > (chanceForAdditionalConnection * chamberA.HallwayConnectors().Count)) continue;

            numOfAdditionalConnectionsToTest--;

            //If chamber B (the supposed unvisited one) is the boss chamber, do not add extra connections
            if (chamberB.IsBossChamber()) continue;

            //Check if chamber A and B's edge connector is still valid
            if (!chamberA.ContainsHallwayConnector(currentEdge.GetEdgeConnectorForChamberA())) continue;
            if (!chamberB.ContainsHallwayConnector(currentEdge.GetEdgeConnectorForChamberB())) continue;

            //Build edge path between chamber A's and B's edge connectors            
            List<GridPosition> path = UsePathfindingAlgorithm(
                currentEdge.GetEdgeConnectorForChamberA(),
                currentEdge.GetEdgeConnectorForChamberB()
                );

            if (path == null)
            {
                //path does not exist. Do not build path
                continue;
            }

            //2) if path is too long, skip the path
            if (path.Count > maxPathLengthForAdditionalConnection) continue;

            BuildEdge(currentEdge, path);

            //Tell chamber A and B their respective edge connector was consumed
            chamberA.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberA());
            chamberB.UseHallwayConnector(currentEdge.GetEdgeConnectorForChamberB());

            //Tell chamber A and B they are connected to one another
            chamberA.AddConnection(chamberB);
            chamberB.AddConnection(chamberA);

            //Extra connection made
            extraConnectionsGenerated++;

            //Tell chamber B to make and add its edges to every unvisited chamber.
            CreateAndQueueEdgesForChamber(chamberB);
        }
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

        List<GridPosition> path = UsePathfindingAlgorithm(bestHallwayConnectorForStartChamber, bestHallwayConnectorForOtherChamber);
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
            if (!IsValidMapPosition(startHallwayConnectorPosition)) { continue; }

            foreach (GridPosition otherHallwayConnectorPosition in otherChamber.HallwayConnectors().Keys.ToList())
            {
                //If the hallway connector is outside the map, then skip it
                if (!IsValidMapPosition(otherHallwayConnectorPosition)) { continue; }

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
    private void CreateAndQueueEdgesForChamber(Chamber givenChamber)
    {
        float timeStartBuildingEdges = Time.realtimeSinceStartup;

        //Loop through all chambers on the map, exclude given chamber & chamber that cannot be connected to
        foreach (Chamber chamber in chambers)
        {
            if (chamber == givenChamber) continue;
            if (!CanConnectChambers(givenChamber, chamber)) continue;
            if (chamber.IsVisited()) continue;

            //Chambers can be connected. Get the best edge connectors for each chamber and its edge cost
            (GridPosition bestChamberAEdgeConnector, GridPosition bestChamberBEdgeConnector) = GetBestEdgeConnectors(givenChamber, chamber);

            List<GridPosition> path = UsePathfindingAlgorithm(bestChamberAEdgeConnector, bestChamberBEdgeConnector);
            int edgeCost = GetEdgeLength(path);

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
            minPQ.Enqueue(edge, edgeCost);
        }

        float timeEndBuildingEdges = Time.realtimeSinceStartup;
        timeSpentBuildingChamberEdges += timeEndBuildingEdges - timeStartBuildingEdges;
    }
    private int GetPathLength(GridPosition chamberAEdgeConnector, GridPosition chamberBEdgeConnector)
    {
        return GridPosition.GetManhattanDistance(chamberAEdgeConnector, chamberBEdgeConnector);
    }
    int GetEdgeLength(List<GridPosition> path)
    {
        float timeStartGettingPathLength = Time.realtimeSinceStartup;
        float timeEndGettingPathLength;

        //Check if path was created
        if (path == null)
        {
            timeEndGettingPathLength = Time.realtimeSinceStartup;
            timeSpentGettingLengthOfPaths += timeEndGettingPathLength - timeStartGettingPathLength;

            return int.MaxValue;
        }

        timeEndGettingPathLength = Time.realtimeSinceStartup;
        timeSpentGettingLengthOfPaths += timeEndGettingPathLength - timeStartGettingPathLength;

        //Get path length
        return path.Count;
    }
    private bool AllChambersVisited()
    {
        //Loop through all chambers. If a single one has not been visited, return false
        foreach (Chamber chamber in chambers)
        {
            if (!chamber.IsVisited()) return false;
        }

        return true;
    }
    private void BuildEdge(Edge edge, List<GridPosition> path)
    {
        edges.Add(edge); //Microsoft Edge

        //Note: path is built from chamber A to B.
        Chamber chamberA = edge.GetChamberA();
        Chamber chamberB = edge.GetChamberB();

        float timeStartBuildingPath = Time.realtimeSinceStartup;

        //TODO: update this to handle path corners, and replace temp prefab with actual hallway prefabs

        //Create parent edge game object
        GameObject edgeObj = new GameObject();
        GameObject parentEdgeRenderer = new GameObject();

        //Initialize parent renderer
        parentEdgeRenderer.transform.parent = edgeObj.transform;
        parentEdgeRenderer.name = "Parent Renderer";

        //Initialize edge object
        edgeObj.name = $"Edge {chamberA.OriginGridPosition()} <-> {chamberB.OriginGridPosition()}";
        EdgeMonoBehaviour edgeMonoBehaviour = edgeObj.AddComponent<EdgeMonoBehaviour>();
        //Loop through each path position
        for (int pathIndex = 0; pathIndex < path.Count; pathIndex++)
        {
            //Get previous position of path. If the previous position is out of bounds, then get Chamber A's hallway position to connect to
            GridPosition previousPosition = pathIndex == 0 ?
                chamberA.GetPositionOfChamberConnectorFromHallwayPosition(path[0]) : path[pathIndex - 1];

            //Get current position
            GridPosition currentPosition = path[pathIndex];

            //Get next posiiton. If out of bounds, get chamber B's hallway position to connect to
            GridPosition nextPosition = pathIndex == path.Count - 1 ?
                chamberB.GetPositionOfChamberConnectorFromHallwayPosition(path[path.Count - 1]) : path[pathIndex + 1];

            //Determine which prefab to use
            GameObject hallwayPrefab = GetHallwayPrefabFromGridPositions(previousPosition, currentPosition, nextPosition);

            //Create the hallway object
            GameObject hallwayObj = Instantiate(hallwayPrefab, GetWorldPositionFromGridPosition(path[pathIndex]), Quaternion.identity);
            hallwayObj.name = $"{path[pathIndex]} Hallway";
            hallwayObj.transform.parent = parentEdgeRenderer.transform;

            //update the map with the new hallway objects
            map[path[pathIndex].x, path[pathIndex].z].MakeObjectAHallway();
        }

        edgeMonoBehaviour.Initialize(edge, parentEdgeRenderer);

        float timeEndBuildingPaths = Time.realtimeSinceStartup;
        timeSpentBuildingPaths += timeEndBuildingPaths - timeStartBuildingPath;
    }
    private GameObject GetHallwayPrefabFromGridPositions(GridPosition previousHallwayPosition, GridPosition currentPosition, GridPosition nextPosition)
    {
        //Default value
        GameObject gameObject = hallwayForwardPlaceholder;

        /*
         * Determine the direction that the hallway will be facing
         * Forward if previous is below and current is above, or vise versa
         * Rightward if previous is left and current is right, or vise versa
         * right up if previous is from right and current is up, or vise vers
         * and so on.
         * 
         * Calculation:
         * previous - current gives direction for up/down/left/right. (prevDir)
         * next - current gives another direction for up/down/left/right. (nextDir)
         * 
         * component = prevDir + nextDir
         * 
         * Caution:
         * if previous and current on same horizontal value, their values cancel to (0,0). Same with it vertically.
         * You cannot differentiate its difference from the conditions when it is vertical or horizontal based on this check along
         * 
         * Check:
         * if component is (0,0), check if prevDir or nextDir x-value is 0. If so, then its a forward component. else it's a rightward component
         */

        GridPosition prevDir = previousHallwayPosition - currentPosition;
        GridPosition nextDir = nextPosition - currentPosition;
        GridPosition componentDir = prevDir + nextDir;

        //Store the proper hallway prefab
        if (componentDir == GridPosition.LeftForward)
        {
            gameObject = hallwayLeftForwardPlaceholder;
        }
        else if (componentDir == GridPosition.RightForward)
        {
            gameObject = hallwayRightForwardPlaceholder;
        }
        else if (componentDir == GridPosition.LeftBackward)
        {
            gameObject = hallwayLeftBackwardPlaceholder;
        }
        else if (componentDir == GridPosition.RightBackward)
        {
            gameObject = hallwayRightBackwardPlaceholder;
        }
        else
        {
            if (prevDir.x != 0)
            {
                //The hallway is horizontal
                gameObject = hallwayRightwardPlaceholder;
            }
            else
            {
                //The hallway is vertical
                gameObject = hallwayForwardPlaceholder;
            }
        }

        /*
            * Fun fact: the code below doesn't work because structs cannot be used in switch-cases... even if you make it equatable...............
            *         
        switch (componentDir)
        {
            case GridPosition.LeftUp:

                break;
            case GridPosition.RightUp:

                break;
            case GridPosition.LeftDown:

                break;
            case GridPosition.RightDown:

                break;
            case GridPosition.Zero:

                break;
        }
            */
        return gameObject;
    }



    //Pathfinding Algorithms
    private List<GridPosition> UsePathfindingAlgorithm(GridPosition gridPosition1, GridPosition gridPosition2)
    {
        List<GridPosition> path = new List<GridPosition>();

        //Use the requested pathfinding algorithm
        switch (pathfindingAlgorithmToUse)
        {
            case PathfindingAlgorithm.AStar:
                path = PerformAStar(gridPosition1, gridPosition2);
                break;
            case PathfindingAlgorithm.BreadthFirstSearch:
                path = PerformBFS(gridPosition1, gridPosition2);
                break;
            case PathfindingAlgorithm.JumpPointSearch:
                path = PerformJumpPointSearch(gridPosition1, gridPosition2);
                break;
            default:
                path = PerformAStar(gridPosition1, gridPosition2);
                break;
        }

        return path;
    }
    private List<GridPosition> PerformAStar(GridPosition chamberAEdgeConnector, GridPosition chamberBEdgeConnector)
    {
        //Increment last visit ID for unique run
        lastVisitID++;

        float TimeStartedAStar = Time.realtimeSinceStartup;

        List<GridPosition> path = new List<GridPosition>();

        //Perform A*
        MinPriorityQueue<(int, int)> frontier = new MinPriorityQueue<(int, int)>();

        //Get starting object
        GridObject startingObj = map[chamberAEdgeConnector.x, chamberAEdgeConnector.z];

        //If the starting position is the ending position, then there is no need to continue pathfinding
        if (chamberAEdgeConnector == chamberBEdgeConnector)
        {
            map[chamberBEdgeConnector.x, chamberBEdgeConnector.z].connectedTo = map[chamberAEdgeConnector.x, chamberAEdgeConnector.z];
            path.Add(chamberAEdgeConnector);
            return path;
        }


        //Set starting Obj gCost, fCost, and hCost
        startingObj.ResetPathfindingProperties();
        startingObj.gCost = 0;
        startingObj.hCost = GridPosition.GetManhattanDistance(chamberBEdgeConnector, startingObj.gridPosition);
        startingObj.CalculateFCost();

        frontier.Enqueue((startingObj.gridPosition.x, startingObj.gridPosition.z), startingObj.fCost);

        //Search for end
        bool pathWasCreated = false;
        int loops = 1000000;
        (int x, int z) currentPos = (startingObj.gridPosition.x, startingObj.gridPosition.z);
        while (frontier.Count() > 0 && loops > 0)
        {
            loops--;
            currentPos = frontier.Dequeue();
            GridObject currentObj = map[currentPos.x, currentPos.z];

            //If the current position is the end, then pathfinding is complete
            if (currentPos.x == chamberBEdgeConnector.x && currentPos.z == chamberBEdgeConnector.z)
            {
                //Path created
                pathWasCreated = true;
                break;
            }

            //Check if better path was already found
            if (currentObj.isClosed)
            {
                continue;
            }

            //Current object has been explored
            currentObj.isClosed = true;

            //Explore neighbors
            foreach (GridPosition relativeNeighborPos in adjacentGridPositions)
            {
                GridPosition absoluteNeighborGridPos = relativeNeighborPos + new GridPosition(currentPos.x, currentPos.z);

                //Conditions to ignore position
                if (!IsValidMapPosition(absoluteNeighborGridPos))
                {
                    continue;
                }

                GridObject neighborGridObj = map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z];

                if (!neighborGridObj.isHallwayPlaceable)
                {
                    continue;
                }
                if (neighborGridObj.isPartOfAChamberHallway && absoluteNeighborGridPos != chamberBEdgeConnector)
                {
                    continue;
                }


                int tentativeGCost = currentObj.gCost + 1;

                //Check if the neighbor was visited in this A* run
                if (neighborGridObj.lastVisitedID != lastVisitID)
                {
                    //Reset obj for pathfinding
                    neighborGridObj.ResetPathfindingProperties();
                    neighborGridObj.lastVisitedID = lastVisitID;
                }

                //Check if the neighbor node is closed
                if (neighborGridObj.isClosed)
                {
                    continue;
                }

                //If the # of "steps" from start to neighbor node is less than the previous steps stored, then this is a better path to that neighbor. Calculate fCost & queue up
                if (tentativeGCost < neighborGridObj.gCost)
                {
                    //Calculate A* data
                    neighborGridObj.gCost = tentativeGCost;
                    neighborGridObj.hCost = GridPosition.GetManhattanDistance(chamberBEdgeConnector, neighborGridObj.gridPosition);
                    neighborGridObj.CalculateFCost();

                    //Connect neighbor to current object
                    neighborGridObj.connectedTo = currentObj;

                    //Neighbor was visited in this A* run
                    neighborGridObj.lastVisitedID = lastVisitID;

                    //Queue for exploration
                    frontier.Enqueue((absoluteNeighborGridPos.x, absoluteNeighborGridPos.z), neighborGridObj.fCost);
                }
            }
        }
        if (loops <= 0)
        {
            Debug.LogError("Error: Looped too many times while performing A* algorithm!");
        }

        if (!pathWasCreated)
        {
            //Path does not exist
            return null;
        }

        //Build the path
        while (currentPos.x != chamberAEdgeConnector.x || currentPos.z != chamberAEdgeConnector.z)
        {
            path.Add(new GridPosition(currentPos.x, currentPos.z));
            GridPosition nextPosition = map[currentPos.x, currentPos.z].connectedTo.gridPosition;
            currentPos = (nextPosition.x, nextPosition.z);
        }

        //Add the starting edge connector (not included in looping process)
        path.Add(chamberAEdgeConnector);
        //Reverse order from chamber A to chamber B
        path.Reverse();

        float TimeEndAStar = Time.realtimeSinceStartup;
        timeSpentPerformingAStar += TimeEndAStar - TimeStartedAStar;

        return path;
    }
    private List<GridPosition> PerformJumpPointSearch(GridPosition gridPosition1, GridPosition gridPosition2)
    {
        //TODO: Craete jump point search pathfinding algorithm. Faster than A*

        List<GridPosition> path = new List<GridPosition>();



        return path;
    }
    List<GridPosition> PerformBFS(GridPosition chamberAEdgeConnector, GridPosition chamberBEdgeConnector)
    {
        //Increment last visit ID for unique run
        lastVisitID++;

        float timeStartedBFS = Time.realtimeSinceStartup;

        List<GridPosition> path = new List<GridPosition>();

        //Perform BFS
        Queue<(int x, int z)> frontier = new Queue<(int x, int z)>();
        map[chamberAEdgeConnector.x, chamberAEdgeConnector.z].lastVisitedID = lastVisitID;
        frontier.Enqueue((chamberAEdgeConnector.x, chamberAEdgeConnector.z));

        //If the starting position is the ending position, then there is no need to continue pathfinding
        if (chamberAEdgeConnector == chamberBEdgeConnector)
        {
            map[chamberBEdgeConnector.x, chamberBEdgeConnector.z].connectedTo = map[chamberAEdgeConnector.x, chamberAEdgeConnector.z];
            path.Add(chamberAEdgeConnector);
            return path;
        }

        (int x, int z) currentPos = (chamberAEdgeConnector.x, chamberAEdgeConnector.z);

        bool pathWasCreated = false;
        int loops = 1000000; //failsafe
        while (frontier.Count > 0 && loops > 0)
        {
            loops--;
            currentPos = frontier.Dequeue();

            //If the current position is the end, then pathfinding is complete
            if (currentPos.x == chamberBEdgeConnector.x && currentPos.z == chamberBEdgeConnector.z)
            {
                //Path created
                pathWasCreated = true;
                break;
            }

            //Explore neighbors
            foreach (GridPosition relativeNeighborPos in adjacentGridPositions)
            {
                GridPosition absoluteNeighborGridPos = relativeNeighborPos + new GridPosition(currentPos.x, currentPos.z);

                //Conditions to ignore position
                if (!IsValidMapPosition(absoluteNeighborGridPos))
                {
                    continue;
                }
                //Already visited on this BFS algorithm
                if (map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z].lastVisitedID == lastVisitID)
                {
                    continue;
                }
                if (!map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z].isHallwayPlaceable)
                {
                    continue;
                }
                if (map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z].isPartOfAChamberHallway && absoluteNeighborGridPos != chamberBEdgeConnector)
                {
                    continue;
                }

                //Neighbor position explored
                map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z].lastVisitedID = lastVisitID;
                //Connect to current pos
                map[absoluteNeighborGridPos.x, absoluteNeighborGridPos.z].connectedTo = map[currentPos.x, currentPos.z];

                //Queue for exploration
                frontier.Enqueue((absoluteNeighborGridPos.x, absoluteNeighborGridPos.z));
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
        while (currentPos.x != chamberAEdgeConnector.x || currentPos.z != chamberAEdgeConnector.z)
        {
            path.Add(new GridPosition(currentPos.x, currentPos.z));
            GridPosition nextPosition = map[currentPos.x, currentPos.z].connectedTo.gridPosition;
            currentPos = (nextPosition.x, nextPosition.z);
        }

        //Add the starting edge connector (not included in looping process)
        path.Add(chamberAEdgeConnector);
        //Reverse order from chamber A to chamber B
        path.Reverse();

        float timeEndedBFS = Time.realtimeSinceStartup;
        timeSpentPerformingBFS += timeEndedBFS - timeStartedBFS;

        return path;
    }



    //Miscellaneous Methods
    Vector3 GetWorldPositionFromGridPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x * mapScale, 0, gridPosition.z * mapScale);
    }
    bool IsValidMapPosition(GridPosition pos)
    {
        //Negative values invalid
        if (pos.x < 0 || pos.z < 0)
        {
            return false;
        }
        //Outside upper bounds
        if (pos.x >= mapLength || pos.z >= mapWidth)
        {
            return false;
        }

        return true;
    }
}