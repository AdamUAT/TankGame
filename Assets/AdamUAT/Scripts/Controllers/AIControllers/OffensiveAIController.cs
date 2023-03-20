using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class OffensiveAIController : AIController
{
    
    [SerializeField]
    protected float wanderRadiusMax = 30; //How far away the next point on the map the tank will wander towards.
    [SerializeField]
    protected float wanderRadiusMin = 15; //The minimum distance the tank will wander.

    protected override void Start()
    {
        //This must be called before any state changes, because it initializes the state to Scan in case something goes wrong.
        base.Start();


        //Initializes the timer and starts the tank moving at the beginning.
        if (FindWanderTarget(20))
        {
            ChangeState(AIState.Wander); 
        }
        else
        {
            ChangeState(AIState.Scan);
        }

    }

    protected override void Update()
    {
        base.Update();
    }

    public override void ChangeState(AIState newState)
    {
        //Calls the end states.
        switch (currentState)
        {
            case AIState.Wander:
                EndWanderState();
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
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }

        base.ChangeState(newState); //Changes the state and updates the timer.

        //Calls the begin states.
        switch(newState)
        {
            case AIState.Scan:
                break;
            case AIState.Wander:
                StartWanderState();
                break;
            case AIState.Chase:
                break;
            case AIState.Search:
                break;
            case AIState.Alert:
                break ;
            case AIState.Flee:
                StartFleeState();
                break;
            case AIState.Idle:
                break;
            default:
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }
    }

    /// <summary>
    /// Calls the behavior that matches the current state.
    /// </summary>
    public override void MakeDecisions()
    {
        switch(currentState)
        {
            case AIState.Wander:
                DoWanderState();
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
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }
    }

    #region Wander
    /// <summary>
    /// If the enemy has reached the point it was wandering to, change state to Scan.
    /// </summary>
    protected void DoWanderState()
    {
        //The check takes into consider floating-point errors by checking if the distance is less-than.
        if (Vector2.Distance(new Vector2(targetLocation.x, targetLocation.z), new Vector2(transform.position.x, transform.position.z)) <= 0.1)
        {
            ChangeState(AIState.Scan); //When the enemy reaches its wandering target, it will idle for a few seconds.
        }

        //Checks to see if the AI can see the player
        if(CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
            return; //Do not 
        }

        //Semi-random movement of the turret.
        DoLookSubstate();
    }

    /// <summary>
    /// Choose a new target location for the tank to wander towards. 
    /// </summary>
    protected void StartWanderState()
    {
        if (!FindWanderTarget(10)) //If it failed to find a target in 10 tries, go back to idling.
        {
            ChangeState(AIState.Scan);
        }

        StartLookSubstate();
    }

    /// <summary>
    /// Cancels the current route of the NavMeshAgent
    /// </summary>
    protected void EndWanderState()
    {
        pawn.mover.StopMoving();
    }

    /// <summary>
    /// Attempts to find a valid position for the enemy to wander to.
    /// </summary>
    /// <param name="limit">The number of times it will check to see if a position is valid.</param>
    /// <returns>Returns true if it found a valid position.</returns>
    protected virtual bool FindWanderTarget(int limit)
    {
        for (int i = 0; i < limit; i++)
        {
            //This will randomly select a spot. If it doens't find a spot within 10 tries, then it will turn to idle state.
            //In addition to randomizing direction, it also randomizes the distance.
            float randomDistance = Random.Range(wanderRadiusMin, wanderRadiusMax);
            Vector3 randomDirection = Random.insideUnitCircle.normalized;
            randomDirection = new Vector3(randomDirection.x, 0, randomDirection.y); //This rearanges the vector3, as the randomization creates a vector 2, so it assignes the z-value to the y-value.
            Vector3 randomTarget = randomDirection * randomDistance + transform.position; //Makes the randomTarget vector have its origin at the player. 

            NavMeshHit hit;

            //Returns true if the point is within 5 units of the randomTarget. If false, it will make another check. This prevents the AI's from being more likely to hug the areas of the map.
            if (NavMesh.SamplePosition(randomTarget, out hit, 5, NavMesh.AllAreas))
            {
                targetLocation = hit.position;
                Seek(targetLocation);
                return true;
            }

        }
        return false;
    }

    #endregion Wander
}