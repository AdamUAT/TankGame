using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This personality only navigates to patrol waypoints in the world.
/// </summary>
public class OffensivePatrolAIController : OffensiveAIController
{
    private List<GameObject> allWaypoints = new List<GameObject>();

    protected override void Start()
    {
        MapGenerator map = FindObjectOfType<MapGenerator>();
        if(map != null )
        {
            //Adds every single waypoint in the map to this tank.
            foreach( Room room in map.roomGrid ) 
            {
                foreach(PatrolWaypoints patrolWaypoints in room.patrolWaypoints)
                {
                    foreach(GameObject waypoint in patrolWaypoints.waypoint)
                    {
                        allWaypoints.Add(waypoint);
                    }
                }
            }

        }

        base.Start();
    }

    /// <summary>
    /// Finds a valid patrol point to navigate to.
    /// </summary>
    /// <param name="limit">How many times it will try to find a valid point before it gives up.</param>
    /// <returns>Whether or not it found a valid point.</returns>
    protected override bool FindWanderTarget(int limit)
    {
        int waypoint;
        for (int i = 0; i < limit; i++)
        {
            //This will randomly select a waypoint.
            waypoint = UnityEngine.Random.Range(0, allWaypoints.Count);

            if (Vector3.Distance(allWaypoints[waypoint].transform.position, pawn.transform.position) >= wanderRadiusMin && Vector3.Distance(allWaypoints[waypoint].transform.position, pawn.transform.position) <= wanderRadiusMax)
            {
                targetLocation = allWaypoints[waypoint].transform.position;
                Seek(targetLocation);
                return true;
            }
        }
        return false;
    }
}
