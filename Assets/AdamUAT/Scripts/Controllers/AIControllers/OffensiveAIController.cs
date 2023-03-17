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
    [SerializeField]
    [Tooltip("Half the arc, in degrees, that the tank's turret can face the player and shoot.")]
    protected float shootFOV = 5;
    #region Turret Movement
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
    protected float sideWhichSide; //Saves which side the turret would look at during the Side state.
    [SerializeField]
    [Tooltip("The angular speed of the turret during the Casual and Narrow states.")]
    protected float slowTurretMoveSpeed = 33;
    [SerializeField]
    [Tooltip("The angular speed of the turret during the Back, Side, and Straight states.")]
    protected float normalTurretMoveSpeed = 45;
    [SerializeField]
    [Tooltip("The angular speed of the turret during the Paranoid state and during combat., in degrees per second")]
    protected float fastTurretMoveSpeed = 90;
    [SerializeField]
    [Tooltip("How far the turret will randomize during Narrow state, half the arc.")]
    protected float narrowDeltaAngle = 33;
    [SerializeField]
    [Tooltip("How far the turret will randomize during Casual state, half the arc.")]
    protected float casualDeltaAngle = 66;
    [SerializeField]
    [Tooltip("How far the turret's randomize is excluded during the Paranoid state, in degrees.")]
    [Range(0f, 180f)]
    protected float paranoidExclusionAngle = 45;
    [SerializeField]
    [Tooltip("How far the turret will oscilate during the Paranoid state, in degrees.")]
    protected float paranoidDeltaAngle = 10;
    [SerializeField]
    [Tooltip("How fast the turret will oscilate during the Paranoid state, in seconds.")]
    protected float paranoidDeltaPeriod = 1;
    [SerializeField]
    [Tooltip("What angle the turret considers the \"side\".")]
    protected float sideSideAngle = 90;
    [SerializeField]
    [Tooltip("How far the turret will randomize during Side state, half the arc.")]
    protected float sideDeltaAngle = 66;
    [SerializeField]
    [Tooltip("How far the turret will randomize during Back state, half the arc.")]
    protected float backDeltaAngle = 66;
    [SerializeField]
    [Tooltip("The minimum duration the turret will stay on a single angle during the Narrow state")]
    protected float minNarrowAngleDuration = 0.25f;
    [SerializeField]
    [Tooltip("The maximum duration the turret will stay on a single angle during the Narrow state")]
    protected float maxNarrowAngleDuration = 1;
    [SerializeField]
    [Tooltip("The minimum duration the turret will stay on a single angle during the Casual state")]
    protected float minCasualAngleDuration = 0.25f;
    [SerializeField]
    [Tooltip("The maximum duration the turret will stay on a single angle during the Casual state")]
    protected float maxCasualAngleDuration = 1;
    [SerializeField]
    [Tooltip("The minimum duration the turret will stay on a single angle during the Paranoid state")]
    protected float minParanoidAngleDuration = 0.5f;
    [SerializeField]
    [Tooltip("The maximum duration the turret will stay on a single angle during the Paranoid state")]
    protected float maxParanoidAngleDuration = 1.25f;
    [SerializeField]
    [Tooltip("The minimum duration the turret will stay on a single angle during the Paranoid state")]
    protected float minSideAngleDuration = 0.25f;
    [SerializeField]
    [Tooltip("The maximum duration the turret will stay on a single angle during the Paranoid state")]
    protected float maxSideAngleDuration = 1; 
    [SerializeField]
    [Tooltip("The minimum duration the turret will stay on a single angle during the Paranoid state")]
    protected float minBackAngleDuration = 0.25f;
    [SerializeField]
    [Tooltip("The maximum duration the turret will stay on a single angle during the Paranoid state")]
    protected float maxBackAngleDuration = 1;
    #endregion Turret Movement

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
            case AIState.Idle:
                //A default state that doesn't do anything.
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
                lastTargetLocation = playerPosition;
                ChangeState(AIState.Alert);
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
                    pawn.mover.TurretRotateAngle(0, normalTurretMoveSpeed);
                    break;
                case lookState.narrow:
                    //new random angle every few seconds. Turret almost never stops moving.
                    if(Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(minNarrowAngleDuration, maxNarrowAngleDuration);
                        turretAngleGoal = Random.Range(-narrowDeltaAngle, narrowDeltaAngle);
                    }
                    else
                    {
                        //It's slow.
                        pawn.mover.TurretRotateAngle(turretAngleGoal, slowTurretMoveSpeed);
                    }
                    break;
                case lookState.casual:
                    //new random angle every few seconds. Turret almost never stops moving.
                    if (Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(minCasualAngleDuration, maxCasualAngleDuration);
                        turretAngleGoal = Random.Range(-casualDeltaAngle, casualDeltaAngle);
                    }
                    else
                    {
                        pawn.mover.TurretRotateAngle(turretAngleGoal, slowTurretMoveSpeed);
                    }
                    break;
                case lookState.paranoid:
                    //new random angle every few seconds. Turret almost never stops moving.
                    if (Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(minParanoidAngleDuration, maxParanoidAngleDuration);
                        turretAngleGoal += Random.Range(0 + paranoidExclusionAngle, 360 - paranoidExclusionAngle); //The turret could look in any direction, but it will look at least some degrees away from its current angle.
                    }
                    else
                    {
                        pawn.mover.TurretRotateAngle(turretAngleGoal + paranoidDeltaAngle * Mathf.Sin(Time.time / paranoidDeltaPeriod * Mathf.PI * 2), fastTurretMoveSpeed);
                    }
                    break;
                case lookState.side:
                    //Side is essentially casual but on either the left or right side.
                    if (Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(minCasualAngleDuration, maxCasualAngleDuration);
                        turretAngleGoal = Random.Range(-casualDeltaAngle, casualDeltaAngle) + sideWhichSide;
                    }
                    else
                    {
                        pawn.mover.TurretRotateAngle(turretAngleGoal, normalTurretMoveSpeed);
                    }
                    break;
                case lookState.back:
                    //Back is essentially casual but 180.
                    if (Time.time > turretTimer)
                    {
                        //Create a new angle and timer.
                        turretTimer = Time.time + Random.Range(minCasualAngleDuration, maxCasualAngleDuration);
                        turretAngleGoal = Random.Range(-casualDeltaAngle, casualDeltaAngle) + 180;
                    }
                    else
                    {
                        pawn.mover.TurretRotateAngle(turretAngleGoal, normalTurretMoveSpeed);
                    }
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
                turretTimer = Time.time + Random.Range(minNarrowAngleDuration, maxNarrowAngleDuration);
                //Randomizes the angle of the turret. It's a narrower version of casual.
                turretAngleGoal = Random.Range(-narrowDeltaAngle, narrowDeltaAngle);
                break;
            case lookState.casual:
                //randomizes how long the first angle will last.
                turretTimer = Time.time + Random.Range(0.25f, 1);
                //Randomizes the angle of the turret.
                turretAngleGoal = Random.Range(-casualDeltaAngle, casualDeltaAngle);
                break;
            case lookState.paranoid:
                //new random angle every few seconds.
                turretTimer = Time.time + Random.Range(0.5f, 1.25f);
                turretAngleGoal += Random.Range(0 + paranoidExclusionAngle, 360 - paranoidExclusionAngle); //The turret could look in any direction, but it will look at least 45 degrees away from its current angle.
                break;
            case lookState.side:
                //Also randomizes a side the turret will look at.
                turretTimer = Time.time + Random.Range(minCasualAngleDuration, maxCasualAngleDuration);
                sideWhichSide = (Random.value < 0.5f ? -1 : 1) * sideSideAngle;
                turretAngleGoal = Random.Range(-casualDeltaAngle, casualDeltaAngle) + sideWhichSide;
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
        Seek(new Vector3(-45, 0.5f, -6.5f));
        return true;

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

    /// <summary>
    /// The enemy pauses and checks left and right for the player, then starts to wander again.
    /// </summary>
    public void DoScanState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }

        if (Time.time >= lastStateChangeTime + 8.0f)
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
    }


    #endregion Wander

    #region Combat
    protected void StartChaseState()
    {

    }
    protected void DoChaseState()
    {
        if(CanSee(target.GetComponent<TankMover>().turret))
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
            }

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
        else if(Time.time <= lastStateChangeTime + 1)
        {
            //Will continue tracking the player for 1 second after loosing sight.
            Seek(target);
            pawn.mover.TurretRotateTowards(target.transform.position, fastTurretMoveSpeed);

        }
        else if(Vector3.Distance(lastTargetLocation, target.transform.position) < 5)
        {
            //The player is close enough to the lastTargetLocation, he is probably behind cover and not running away.
            pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);
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
        if (CanSee(target.GetComponent<TankMover>().turret))
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
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }


        if(Time.time <= lastStateChangeTime + 1.5f && Time.time >= lastStateChangeTime)
        {
            pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);
        }
        else if(Vector3.Distance(transform.position, lastTargetLocation) < 7.5) //If the enemy is close enough to the player, it will move to where it heard the player.
        {
            ChangeState(AIState.Search);
        }
        if (Time.time >= lastStateChangeTime + 10.0f)
        {
            ChangeState(AIState.Wander);
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
    protected void EndAlertState() 
    {
        
    }
    #endregion Combat
    
}