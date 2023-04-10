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
        if (CanSeePlayer())
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
        if (CanSeePlayer())
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

    protected override void DoFleeState()
    {
        if (pawn.health.currentHealth / pawn.health.maxHealth > fleeThreshold)
        {
            if (CanSeePlayerNoFOV()) //Checks to see if the player was chasing this pawn to the health pickup. If so, then it resumes the chase.
            {
                ChangeState(AIState.Chase);
            }
            else
            {
                ChangeState(AIState.Patrol);
            }
        }
        else if (Vector2.Distance(new Vector2(targetLocation.x, targetLocation.z), new Vector2(transform.position.x, transform.position.z)) <= 0.1)
        {
            NavigateToClosestHealthPowerup(); //The health is still too low after getting a health pickup, then it navigates to the next one.
        }

        //If the player is chasing the tank while fleeing, the tank will still shoot back.
        if (CanSeePlayerNoFOV())
        {
            pawn.mover.TurretRotateTowards(target.transform.position, fastTurretMoveSpeed);

            //If the tank is looking at the tank, then it will shoot the tank.
            Vector3 aiToTarget = target.transform.position - pawn.mover.turret.transform.position;
            //Makes it so the angle is 2d.
            float angleToTargetFromTurret = Vector2.Angle(new Vector2(aiToTarget.x, aiToTarget.z), new Vector2(pawn.mover.turret.transform.forward.x, pawn.mover.turret.transform.forward.z)); //Need to have the turret angle seperate from the body, otherwise stuff gets weird.
            //Allows a 5 degree margin
            if (Mathf.Abs(angleToTargetFromTurret) <= shootFOV)
            {
                pawn.shooter.Shoot();
            }
        }
    }

    protected override void DoSearchState()
    {
        if (CanSeePlayer())
        {
            ChangeState(AIState.Chase);
        }

        //Moves the enemy to the last known location of the player.
        Seek(lastTargetLocation);

        //Keep the turret pointed towards the destination.
        pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);

        //This should make sure enough time has passed that the frames have calculated enough for IsMoving to actually return a real result.
        if (Time.time >= lastStateChangeTime + 0.1)
        {
            //After the enemy arrived at the point, it looks around for the player. If it doesn't find it, then it starts to wander agian.
            if (!pawn.mover.IsMoving())
            {
                ChangeState(AIState.Patrol);
            }
        }
    }
}
