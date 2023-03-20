using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour {

	public GameObject doorNorth;
	public GameObject doorSouth;
	public GameObject doorEast;
	public GameObject doorWest;

	public GameObject playerSpawn;

	public PatrolWaypoints[] patrolWaypoints;
	public GameObject[] wanderSpawns;
	public GameObject[] guardSpawns;
	public Pickup[] pickups;
	
}

[System.Serializable]
public class PatrolWaypoints
{
	public GameObject[] waypoint;
}
