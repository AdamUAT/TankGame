using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIController;

public class PatrolAIController : AIController
{
    [Tooltip("The waypoints must be placed in order.")]
    public GameObject[] waypoints;
    [HideInInspector]
    public int currentWaypointTarget;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Starts the state in the patrol state.
        ChangeState(AIState.Patrol);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void ChangeState(AIState newState)
    {
        //Calls the end states.
        switch (currentState)
        {
            case AIState.Patrol:
                break;
            case AIState.Chase:
                break;
            case AIState.Scan:
                break;
            case AIState.Search:
                break;
            case AIState.Alert:
                break;
            case AIState.Flee:
                break;
            case AIState.Idle:
                //Do nothing, since Idle is a placeholder state.
                break;
            default:
                Debug.LogWarning("Patrol Tank had its state go out of bounds.");
                break;
        }

        base.ChangeState(newState); //Changes the state and updates the timer.

        //Calls the begin states.
        switch (newState)
        {
            case AIState.Scan:
                break;
            case AIState.Patrol:
                break;
            case AIState.Chase:
                break;
            case AIState.Search:
                break;
            case AIState.Alert:
                break;
            case AIState.Flee:
                StartFleeState();
                break;
            case AIState.Idle:
                break;
            default:
                Debug.LogWarning("Patrol Tank had its state go out of bounds.");
                break;
        }
    }

    /// <summary>
    /// Calls the behavior that matches the current state.
    /// </summary>
    public override void MakeDecisions()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                DoPatrolState();
                break;
            case AIState.Scan:
                DoScanState();
                break;
            case AIState.Chase:
                DoChaseState();
                break;
            case AIState.Search:
                DoSearchState();
                break;
            case AIState.Alert:
                DoAlertState();
                break;
            case AIState.Flee:
                DoFleeState();
                break;
            case AIState.Idle:
                //A default state that doesn't do anything.
                break;
            default:
                Debug.LogWarning("Patrol Tank had its state go out of bounds.");
                break;
        }
    }

    protected void DoPatrolState()
    {
        //Checks to see if the AI can see the player
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
            return; //Do not 
        }

        //The check takes into consider floating-point errors by checking if the distance is less-than.
        if (Vector2.Distance(new Vector2(waypoints[currentWaypointTarget].transform.position.x, waypoints[currentWaypointTarget].transform.position.z), new Vector2(transform.position.x, transform.position.z)) <= 0.1)
        {
            currentWaypointTarget++;

            //If the currentWaypointTarget was at the end of the array, instead loop to the beginning.
            if (currentWaypointTarget == waypoints.Length)
            {
                currentWaypointTarget = 0;
            }

            //Navigates to the next waypoint.
            Seek(waypoints[currentWaypointTarget]);

            //Semi-random movement of the turret.
            DoLookSubstate();
        }
    }

    protected override void DoAlertState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }


        if (Time.time <= lastStateChangeTime + 1.5f && Time.time >= lastStateChangeTime)
        {
            pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);
        }
        else if (Vector3.Distance(transform.position, lastTargetLocation) < 7.5) //If the enemy is close enough to the player, it will move to where it heard the player.
        {
            ChangeState(AIState.Search);
        }
        if (Time.time >= lastStateChangeTime + 10.0f)
        {
            ChangeState(AIState.Patrol);
        } //If the player is too far away, it will slightly move the turret in the direction it heard the player.
        else if (Time.time <= lastStateChangeTime + 6.0f && Time.time >= lastStateChangeTime + 4.0f)
        {
            pawn.mover.TurretRotate(-15);
        }
        else if (Time.time <= lastStateChangeTime + 3.0f && Time.time >= lastStateChangeTime + 2f)
        {
            pawn.mover.TurretRotate(15);
        }
    }

    protected override void DoScanState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }

        if (Time.time >= lastStateChangeTime + 8.0f)
        {
            ChangeState(AIState.Patrol);
        }
        else if (Time.time <= lastStateChangeTime + 8.0f && Time.time >= lastStateChangeTime + 7.0f)
        {
            pawn.mover.TurretRotate(90); //Returns turret to front position.
        }
        else if (Time.time <= lastStateChangeTime + 6.0f && Time.time >= lastStateChangeTime + 4.0f)
        {
            pawn.mover.TurretRotate(-90);
        }
        else if (Time.time <= lastStateChangeTime + 2.0f && Time.time >= lastStateChangeTime + 1.0f)
        {
            pawn.mover.TurretRotate(90);
        }
    }
}
