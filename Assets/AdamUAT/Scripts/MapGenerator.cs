using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
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
                GameObject roomObj = Instantiate(RandomRoom(), new Vector3(roomDimensions.x * j, 0, roomDimensions.y * i), Quaternion.identity);

                // Make the room a child of the map GameObject.
                roomObj.transform.parent = this.transform;

                // Give it a meaningful name
                roomObj.name = "Room_" + j + "_" + i;

                // Get the room object
                Room room = roomObj.GetComponent<Room>();

                //Open doors in the room.
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

        //
    }

    /// <summary>
    /// Finds a weighted random room from the array of prefabs.
    /// </summary>
    /// <returns>A prefab reference to the chosen room.</returns>
    private GameObject RandomRoom()
    {
        //Gets the total weight values for the powerups.
        float totalWeight = 0;
        foreach (RoomPrefab roomPrefab in roomPrefabs)
        {
            totalWeight += roomPrefab.weight;
        }

        //Chooses a random float in the range of the weights
        float random = Random.Range(0, totalWeight);

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
}

//This is stored in its own class so it can appear together in the inspector.
[System.Serializable]
public class RoomPrefab
{
    public GameObject roomPrefab;
    [Tooltip("The weight that this room has a chance to spawn.")]
    public float weight;
}
