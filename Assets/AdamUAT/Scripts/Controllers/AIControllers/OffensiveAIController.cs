using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    [SerializeField]
    protected float searchDelay = 5; //The delay between when the AI loses sight of the player and moves closer.
    [SerializeField]
    protected float followDistance = 5; //The distance in units the enemy will stay away from the player until moving closer.

    //CANNOT change the int value of the enum, it would mess up how they are assigned.
    protected enum lookState { straight, narrow, casual, paranoid, back, side}
    [SerializeField]
    protected lookState currentLookState;
    [SerializeField]
    protected float[] lookStateWeights = { 0.5f, 1.25f, 2.5f, 0.5f, 1, 1, }; //The weighted odds of which lookState would be chosen.
    [SerializeField]
    protected float minLookTime = 5.0f;
    [SerializeField]
    protected float maxLookTime = 15.0f;
    protected float endLookTime;
    protected float turretAngleGoal;
    protected float turretTimer;

    protected override void Start()
    {
        navMesh = NavMeshManager.instance.globalNavMesh; //Assign the navMesh to be the entire map.

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

        target = GameManager.instance.players[0].pawn.gameObject; //This enemy will only target the player.
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
                EndChaseState();
                break;
            case AIState.Scan:
                break;
            case AIState.Search:
                EndSearchState();
                break;
            case AIState.Alert:
                EndAlertState();
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
                StartChaseState();
                break;
            case AIState.Search:
                StartSearchState();
                break;
            case AIState.Alert:
                StartAlertState();
                break ;
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
            default:
                Debug.LogWarning("Offensive Tank had its state go out of bounds.");
                break;
        }
    }

    public override void HeardPlayerShoot(Vector3 playerPosition)
    {
        //Checks if the enemy is close enough to the player to hear it.
        if (Vector3.Distance(pawn.transform.position, playerPosition) <= hearingRange)
        {
            //If the enemy is in the chase state, then it will ignore the player firing, because it knows where the player is and is most likely firing at it.
            if (currentState != AIState.Chase)
            {
                ChangeState(AIState.Scan);
            }
        }
    }

    #region Wander
    /// <summary>
    /// If the enemy has reached the point it was wandering to, change state to Scan.
    /// </summary>
    protected void DoWanderState()
    {
        //The check takes into consider floating-point errors by checking if the distance is less-than.
        if (Vector3.Distance(targetLocation, transform.position) <= 0.1)
        {
            ChangeState(AIState.Scan); //When the enemy reaches its wandering target, it will idle for a few seconds.
        }

        //Checks to see if the AI can see the player
        if(CanSee(target))
        {
            ChangeState(AIState.Chase);
            return; //Do not 
        }

        //Semi-random movement of the turret.
        DoLookWanderState();
    }

    /// <summary>
    /// A sub-FSM for the Wander state that controlls where the tank turret is looking.
    /// </summary>
    protected void DoLookWanderState()
    {
        if (Time.time < endLookTime)
        {
            switch (currentLookState)
            {
                case lookState.straight:
                    //Keeps the turret facing forward.
                    pawn.mover.TurretRotateAngle(0, 45);
                    break;
                case lookState.narrow:
                    //new random angle every few seconds. Turret almost never stops moving.
                    if(Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(0.25f, 1);
                        turretAngleGoal = Random.Range(-33, 33);
                    }
                    else
                    {
                        //It's slow.
                        pawn.mover.TurretRotateAngle(turretAngleGoal, 33);
                    }
                    break;
                case lookState.casual:
                    Debug.Log("alkdjflak");
                    //new random angle every few seconds. Turret almost never stops moving.
                    if (Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(0.25f, 1);
                        turretAngleGoal = Random.Range(-66, 66);
                    }
                    else
                    {
                        //It's slow.
                        pawn.mover.TurretRotateAngle(turretAngleGoal, 33);
                    }
                    break;
                case lookState.paranoid:
                    break;
                case lookState.side:
                    break;
                case lookState.back:
                    break;
                default:
                    Debug.LogWarning("Offensive Tank's Wander state had its sub-state go out of bounds.");
                    break;

            }
        }
        else
        {
            //Choose a new sub-state to follow.
            StartLookWanderState();
        }
    }

    /// <summary>
    /// Decides which Wander sub-state is active and for how long.
    /// </summary>
    protected void StartLookWanderState()
    {
        //Randomizes how long the turret will stay in a sub-state.
        endLookTime = Time.time + Random.Range(minLookTime, maxLookTime);

        //Gets the total weight values for the states.
        float totalWeight = 0;
        foreach(float weight in lookStateWeights)
        {
            totalWeight += weight;
        }

        //Chooses a random float in the range of the weights
        float random = Random.Range(0, totalWeight);

        //Checks to see which state fits the random number chosen.
        float weightIncrement = 0;
        for(int i = 0; i < lookStateWeights.Length; i++)
        {
            if(weightIncrement <= random && weightIncrement + lookStateWeights[i] >= random)
            {
                //Converts the enum into its integer equivalent. This only works if the enum stays with the default start at 0 increment by 1.
                currentLookState = (lookState)i;
                break; //Stops the rest of the loop from occuring
            }

            weightIncrement += lookStateWeights[i];
        }

        //Starts some of the LookStates
        switch (currentLookState)
        {
            case lookState.straight:
                break;
            case lookState.narrow:
                //randomizes how long the first angle will last.
                turretTimer = Time.time + Random.Range(0.25f, 1);
                //Randomizes the angle of the turret. It's a narrower version of casual.
                turretAngleGoal = Random.Range(-33, 33);
                break;
            case lookState.casual:
                //randomizes how long the first angle will last.
                turretTimer = Time.time + Random.Range(0.25f, 1);
                //Randomizes the angle of the turret.
                turretAngleGoal = Random.Range(-66, 66);
                break;
            case lookState.paranoid:
                break;
            case lookState.side:
                break;
            case lookState.back:
                break;
            default:
                Debug.LogWarning("Offensive Tank's Wander state had its sub-state go out of bounds.");
                break;

        }
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

        StartLookWanderState();
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
    protected bool FindWanderTarget(int limit)
    {
        for (int i = 0; i < 10; i++)
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

    /// <summary>
    /// The enemy pauses and checks left and right for the player, then starts to wander again.
    /// </summary>
    public void DoScanState()
    {
        if(Time.time >= lastStateChangeTime + 8.0f)
        {
            ChangeState(AIState.Wander);
        }
        else if(Time.time <= lastStateChangeTime + 8.0f && Time.time >= lastStateChangeTime + 7.0f)
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

        if(CanSee(target))
        {
            ChangeState(AIState.Chase);
        }
    }


    #endregion Wander

    #region Combat
    protected void StartChaseState()
    {

    }
    protected void DoChaseState()
    {
        if(CanSee(target))
        {
            lastStateChangeTime = Time.time; //Tells the timer when the last time the enemy saw the player was.
            if(DistanceFromTarget() > followDistance)
            {
                //If the distance between the target and the enemy is too big, then the enemy will move towards the player.
                Seek(target);
            }
            else
            {
                //Stops the enemy from getting too close to the player willingly.
                if(pawn.mover.IsMoving())
                {
                    pawn.mover.StopMoving();
                }

                pawn.mover.TurretRotateTowards(target);

                //If the tank is looking at the tank, then it will shoot the tank.
                Vector3 aiToTarget = target.transform.position - transform.position;
                float angleToTarget = Vector3.Angle(aiToTarget, pawn.transform.forward);
                if (Mathf.Abs(angleToTarget) > 5)
                {
                    pawn.shooter.Shoot();
                }
            }
        }
        else if(Time.time >= lastStateChangeTime + searchDelay)
        {
            //Tells the AI to move to the player's last known location if it has gone searchDelay seconds without seeing the player.
            ChangeState(AIState.Search);
        }
        else
        {

        }
    }
    protected void EndChaseState()
    {

    }
    protected void StartSearchState()
    {

    }
    protected void DoSearchState()
    {
        //Moves the enemy to the last known location of the player.
        Seek(lastTargetLocation);

        //This should make sure enough time has passed that the frames have calculated enough for IsMoving to actually return a real result.
        if (Time.time >= lastStateChangeTime + 0.1)
        {
            //After the enemy arrived at the point, it looks around for the player. If it doesn't find it, then it starts to wander agian.
            if (!pawn.mover.IsMoving())
            {
                ChangeState(AIState.Scan);
            }
        }
    }
    protected void EndSearchState()
    {

    }
    protected void StartAlertState() 
    {

    }
    protected void DoAlertState() 
    {
        
    }
    protected void EndAlertState() 
    {
        
    }
    #endregion Combat
    
}