using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Room : MonoBehaviour {

	public GameObject doorNorth;
	public GameObject doorSouth;
	public GameObject doorEast;
	public GameObject doorWest;

	public GameObject playerSpawn;

	public PatrolWaypoints[] patrolWaypoints;
	public GameObject[] offensiveSpawns;
	public GameObject[] guardSpawns;
	public Pickup[] pickups;
	public NavMeshSurface navMesh;
}

[System.Serializable]
public class PatrolWaypoints
{
	public GameObject[] waypoint;
}
