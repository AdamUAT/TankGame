using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OffensiveAIController : AIController
{
    [SerializeField]
    protected float WanderRadiusMax = 30; //How far away the next point on the map the tank will wander towards.
    [SerializeField]
    protected float WanderRadiusMin = 15; //The minimum distance the tank will wander.

    public override void Start()
    {
        navMesh = NavMeshManager.instance.globalNavMesh; //Assign the navMesh to be the entire map.

        //This must be called before any state changes, because it initializes the state to Idle in case something goes wrong.
        base.Start();

        //Initializes the timer and starts the tank moving at the beginning.
        if (FindWanderTarget(20))
        {
            ChangeState(AIState.Wander); 
        }
        else
        {
            ChangeState(AIState.Idle);
        }

    }

    // Update is called once per frame
    public override void Update()
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
            case AIState.Idle:
                break;
            case AIState.Search:
                break;
            default:
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }

        base.ChangeState(newState); //Changes the state and updates the timer.

        //Calls the begin states.
        switch(newState)
        {
            case AIState.Idle:
                break;
            case AIState.Wander:
                StartWanderState();
                break;
            case AIState.Chase:
                break;
            case AIState.Search:
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
            case AIState.Idle:
                DoIdleState();
                break;
            case AIState.Chase:
                break;
            case AIState.Search:
                break;
            default:
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }
    }

    /// <summary>
    /// Sees if it should switch states based on the player.
    /// </summary>
    public override void CheckSenses()
    {
        //Checks if it can see the player.
        target = GameManager.instance.players[0].pawn.gameObject;
        if(CanSee(target))
        {
            ChangeState(AIState.Chase);
        }
    }

    public override void HeardPlayerShoot()
    {
        ChangeState(AIState.Search);
    }

    #region Wander
    /// <summary>
    /// If the enemy has reached the point it was wandering to, change state to Idle.
    /// </summary>
    protected void DoWanderState()
    {
        //The check takes into consider floating-point errors by checking if the distance is less-than.
        if (Vector3.Distance(targetLocation, transform.position) <= 0.1)
        {
            ChangeState(AIState.Idle); //When the enemy reaches its wandering target, it will idle for a few seconds.
        }
    }
    /// <summary>
    /// Choose a new target location for the tank to wander towards. 
    /// </summary>
    protected void StartWanderState()
    {
        if (!FindWanderTarget(10)) //If it failed to find a target in 10 tries, go back to idling.
        {
            ChangeState(AIState.Idle);
        }
    }

    /// <summary>
    /// Cancels the current route of the NavMeshAgent
    /// </summary>
    protected void EndWanderState()
    {
        pawn.StopMoving();
    }

    /// <summary>
    /// Attempts to find a valid position for the enemy to wander to.
    /// </summary>
    /// <param name="limit">The number of times it will check to see if a position is valid.</param>
    /// <returns>Returns true if it found a valid position.</returns>
    protected bool FindWanderTarget(int limit)
    {
        for (int i = 0; i < 10; i++)
        {
            //This will randomly select a spot. If it doens't find a spot within 10 tries, then it will turn to idle state.
            //In addition to randomizing direction, it also randomizes the distance.
            float randomDistance = Random.Range(WanderRadiusMin, WanderRadiusMax);
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

    /// <summary>
    /// The enemy pauses and checks left and right for the player, then starts to wander again.
    /// </summary>
    public override void DoIdleState()
    {
        if(Time.time >= lastStateChangeTime + 7.0f)
        {
            ChangeState(AIState.Wander);
        }
        else if (Time.time <= lastStateChangeTime + 6.0f && Time.time >= lastStateChangeTime + 4.0f)
        {
            pawn.RotateCounterClockwise();
        }
        else if (Time.time <= lastStateChangeTime + 2.0f && Time.time >= lastStateChangeTime + 1.0f)
        {
            pawn.RotateClockwise();
        }
    }


    #endregion Wander

    #region Combat
    //public ov
    #endregion Combat
}
