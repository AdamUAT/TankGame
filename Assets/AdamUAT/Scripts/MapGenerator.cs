using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour
{
    [HideInInspector]
    public Room[,] roomGrid;

    [SerializeField]
    [Tooltip("The prefabs of each room that is possible to spawn.")]
    private RoomPrefab[] roomPrefabs;
    [SerializeField]
    [Tooltip("The number of rows and columns of rooms the grid will have.")]
    private Vector2Int gridSize;
    [SerializeField]
    [Tooltip("The size of each room. This determines how far away the rooms are placed.")]
    private Vector2 roomDimensions;

    [SerializeField]
    [Tooltip("The prefabs of each patrol personality that is possible to spawn.")]
    private EnemyPrefab[] patrolPrefabs;
    [SerializeField]
    [Tooltip("The prefabs of each offensive personality that is possible to spawn.")]
    private EnemyPrefab[] offensivePrefabs;
    [SerializeField]
    [Tooltip("The prefabs of each guard personality that is possible to spawn.")]
    private EnemyPrefab[] guardPrefabs;

    [Tooltip("If checked, then the map will generate the exact same for the entire day.")]
    public bool isSeedByDay = false;

    [Tooltip("If checked, then the seed will not be randomized during runime, allowing the designer to set the seed. isSeedByDay overrules this.")]
    public bool isCustomSeed = false;

    public int seed;

    [Tooltip("The NavMeshSurface that covers the entire map instead of just inside rooms.")]
    public NavMeshSurface globalNavMesh;

    // Start is called before the first frame update
    void Start()
    {
        isSeedByDay = GameManager.instance.isDaySeed;
        isCustomSeed = !GameManager.instance.isRandomSeed;
        seed = GameManager.instance.customSeed;

        if(isSeedByDay)
        {
            UnityEngine.Random.InitState(DateTime.Now.DayOfYear * DateTime.Now.Year); //This makes it so each day has the same seed.
        }
        else if(isCustomSeed)
        {
            UnityEngine.Random.InitState(seed);
        }
        else
        {
            //If the designer wants Unity to seed it itself, still show the designer what the seed was.
            seed = UnityEngine.Random.seed;
        }

        gridSize.x = GameManager.instance.mapColumns;
        gridSize.y = GameManager.instance.mapRows;

        GenerateMap();
    }

    public void GenerateMap()
    {
        // Clear out the grid - "column" is our X, "row" is our Y
        roomGrid = new Room[gridSize.x, gridSize.y];

        //Iterates through each row.
        for (int i = 0; i < gridSize.y; i++)
        {
            //Iterates through each column.
            for (int j = 0; j < gridSize.x; j++)
            {
                // Create a new grid at the appropriate location.
                GameObject roomObj = Instantiate(RandomRoomWeighted(), new Vector3(roomDimensions.x * j, 0, roomDimensions.y * i), Quaternion.identity);

                // Make the room a child of the map GameObject.
                roomObj.transform.parent = this.transform;

                // Give it a meaningful name
                roomObj.name = "Room_" + j + "_" + i;

                // Get the room object
                Room room = roomObj.GetComponent<Room>();

                //Close doors in the room.
                if (j == 0)
                    room.doorWest.SetActive(true);
                else if(j == gridSize.x - 1)
                    room.doorEast.SetActive(true);
                if (i == 0)
                    room.doorSouth.SetActive(true);
                else if (i == gridSize.y - 1)
                    room.doorNorth.SetActive(true);

                // Save it to the grid array
                roomGrid[j, i] = room;


            }
        }

        //After the map is generated, update the globalNavMesh.
        globalNavMesh.BuildNavMesh();

        GameManager.instance.SpawnPlayer();

        //SpawnEnemies must be after the player is spawned so they can target the player.
        foreach(Room room in roomGrid)
        {
            //Spawns the enemy in the room.
            SpawnEnemies(room);
        }
    }

    /// <summary>
    /// Finds a random room in the scene.
    /// </summary>
    /// <returns>A reference to a room on the map.</returns>
    public Room RandomRoom()
    {
        return (roomGrid[UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y)]);
    }

    /// <summary>
    /// Finds a weighted random room from the array of prefabs.
    /// </summary>
    /// <returns>A prefab reference to the chosen room.</returns>
    private GameObject RandomRoomWeighted()
    {
        //Gets the total weight values for the powerups.
        float totalWeight = 0;
        foreach (RoomPrefab roomPrefab in roomPrefabs)
        {
            totalWeight += roomPrefab.weight;
        }

        //Chooses a random float in the range of the weights
        float random = UnityEngine.Random.Range(0, totalWeight);

        //Checks to see which state fits the random number chosen.
        float weightIncrement = 0;
        foreach (RoomPrefab roomPrefab in roomPrefabs)
        {
            if (weightIncrement <= random && weightIncrement + roomPrefab.weight >= random)
            {
                //Returns the prefab associated with the weight.
                return(roomPrefab.roomPrefab);
            }

            weightIncrement += roomPrefab.weight;
        }

        //This returns the first room, which should be empty. This code shouldn't be reached, but just in case.
        return(roomPrefabs[0].roomPrefab);
    }

    /// <summary>
    /// Spawns all the enemies in the room.
    /// </summary>
    /// <param name="room">The room to spawn enemies in.</param>
    private void SpawnEnemies(Room room)
    {
        foreach (PatrolWaypoints patrolWaypoints in room.patrolWaypoints)
        {
            SpawnPatrol(patrolWaypoints.waypoint);
        }
        foreach(GameObject offensiveSpawn in room.offensiveSpawns)
        {
            SpawnOffensive(offensiveSpawn);
        }
        foreach (GameObject guardSpawn in room.guardSpawns)
        {
            SpawnGuard(guardSpawn);
        }
    }

    /// <summary>
    /// Spawns a patrol enemy with a random personality on one of its waypoints.
    /// </summary>
    /// <param name="waypoints">The array of waypoints this patrol enemy will use.</param>
    private void SpawnPatrol(GameObject[] waypoints)
    {
        //Randomizes which waypoint the patrol will spawn at.
        int waypointNumber = UnityEngine.Random.Range(0, waypoints.Length);
        //Randomizes which patrol personality prefab will spawn.
        GameObject personality = RandomPersonalityPrefab(patrolPrefabs);

        //Instantiates the enemy with a random personality at a random waypoint.
        GameObject enemy = Instantiate(personality, waypoints[waypointNumber].transform.position, waypoints[waypointNumber].transform.rotation);

        //Tells the controller which waypoint it was spawned at.
        PatrolAIController controller = enemy.GetComponent<PatrolAIController>();
        if (controller != null)
        {
            controller.currentWaypointTarget = waypointNumber;
            controller.target = GameManager.instance.players[0].gameObject;
            controller.waypoints = waypoints;
        }
        else
            Debug.LogError("Could not find a PatrolAIController on this patrol-type enemy.");
    }

    /// <summary>
    /// Spawns an offensive enemy with a random personality on its spawn.
    /// </summary>
    /// <param name="spawn">The location the offensive enemy will spawn at.</param>
    private void SpawnOffensive(GameObject spawn)
    {
        //Randomizes which offensive personality prefab will spawn.
        GameObject personality = RandomPersonalityPrefab(offensivePrefabs);

        //Instantiates the enemy with a random personality at a random waypoint.
        GameObject enemy = Instantiate(personality, spawn.transform.position, spawn.transform.rotation);

        //Tells the controller which waypoint it was spawned at.
        OffensiveAIController controller = enemy.GetComponent<OffensiveAIController>();

        if (controller != null)
        {
            controller.target = GameManager.instance.players[0].gameObject;
        }
        else
            Debug.LogError("Could not find a OffensiveAIController on this offensive-type enemy.");
    }

    /// <summary>
    /// Spawns an guard enemy with a random personality on its spawn.
    /// </summary>
    /// <param name="spawn">The location the guard enemy will spawn at.</param>
    private void SpawnGuard(GameObject spawn)
    {
        //Randomizes which offensive personality prefab will spawn.
        GameObject personality = RandomPersonalityPrefab(guardPrefabs);

        //Instantiates the enemy with a random personality at a random waypoint.
        GameObject enemy = Instantiate(personality, spawn.transform.position, spawn.transform.rotation);

        //Tells the controller which waypoint it was spawned at.
        GuardAIController controller = enemy.GetComponent<GuardAIController>();

        if (controller != null)
        {
            controller.target = GameManager.instance.players[0].gameObject;
        }
        else
            Debug.LogError("Could not find a GuardAIController on this guard-type enemy.");
    }

    /// <summary>
    /// Randomizes the AI personality 
    /// </summary>
    /// <param name="enemyPrefabs">The array of available personalities.</param>
    /// <returns>A prefab of the personality.</returns>
    private GameObject RandomPersonalityPrefab(EnemyPrefab[] enemyPrefabs)
    {
        //Gets the total weight values for the powerups.
        float totalWeight = 0;
        foreach (EnemyPrefab enemyPrefab in enemyPrefabs)
        {
            totalWeight += enemyPrefab.weight;
        }

        //Chooses a random float in the range of the weights
        float random = UnityEngine.Random.Range(0, totalWeight);

        //Checks to see which state fits the random number chosen.
        float weightIncrement = 0;
        foreach (EnemyPrefab enemyPrefab in enemyPrefabs)
        {
            if (weightIncrement <= random && weightIncrement + enemyPrefab.weight >= random)
            {
                //Returns the prefab associated with the weight.
                return (enemyPrefab.enemyPrefab);
            }

            weightIncrement += enemyPrefab.weight;
        }

        //This returns the first room, which should be empty. This code shouldn't be reached, but just in case.
        return (enemyPrefabs[0].enemyPrefab);
    }
}

//These are stored in its own classes so their data can appear together in the inspector.
[System.Serializable]
public class RoomPrefab
{
    public GameObject roomPrefab;
    [Tooltip("The weight that this room has a chance to spawn.")]
    public float weight;
}

[System.Serializable]
public class EnemyPrefab
{
    public GameObject enemyPrefab;
    [Tooltip("The weight that this personality has a chance to spawn.")]
    public float weight;
}
