using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AIController : Controller
{
    
    public enum AIState { Idle, Guard, Chase, Flee, Patrol, Attack, Scan, BackToPost, Wander, Search, Alert}


    public AIState currentState;
    protected float lastStateChangeTime;
    public GameObject target; //This IS NOT equivilant to the player. This is any gameobject the AI is set to move to.
    protected Vector3 targetLocation; //This is similiar to target, but is a set location instead of a moving GameObject.
    protected Vector3 lastTargetLocation; //If the AI loses sight of the target, it knows the last spot it saw the target.

    private bool sightCache; //This holds the past result of CanSee.
    private float sightCacheTimer; //A timer that controlls when CanSee will recaculate vs. return the previous calculation.
    [SerializeField]
    private float sightCacheDelay = 0.25f; //The variable that controlls how often this AI checks for vision.

    [SerializeField]
    protected float hearingRange = 20.0f;
    [SerializeField]
    protected float bodyFieldOfView = 45.0f; //The angle of half of the AI's field of view, in degrees. The FOV is centered on the front face of the body.
    [SerializeField]
    protected float turretFieldOfView = 60.0f; //The angle of half of the AI's field of view, in degrees. The FOV is centered on the forward of the turret.
    [SerializeField]
    protected float eyesight = 15f; //The range the tanks are able to see the player.

    [SerializeField]
    [Tooltip("The percentage of health this tank will enter the flee from the player.")]
    protected float fleeThreshold = 0.5f;

    [SerializeField]
    protected float searchDelay = 5; //The delay between when the AI loses sight of the player and moves closer.
    [SerializeField]
    protected float followDistance = 5; //The distance in units the enemy will stay away from the player until moving closer.
    [SerializeField]
    [Tooltip("Half the arc, in degrees, that the tank's turret can face the player and shoot.")]
    protected float shootFOV = 5;

    [SerializeField]
    [Tooltip("Begins showing debug gizmos for the AI")]
    protected bool debugMode = false;

    #region Turret Movement
    //CANNOT change the int value of the enum, it would mess up how they are assigned.
    protected enum lookState { straight, narrow, casual, paranoid, back, side }
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


    protected virtual void Start()
    {
        currentState = AIState.Idle;

        //This is in the AIController class instead of Controller class because the player's controller is spawned on a different GameObject than the player at runtime.
        pawn = GetComponent<TankPawn>();
        GameManager.instance.npcs.Add(this);
        target = GameManager.instance.players[0].pawn.gameObject; //This enemy will only target the player.

    }

    protected virtual void Update()
    {
        MakeDecisions();
    }

    public virtual void MakeDecisions()
    {

    }

    /// <summary>
    /// Changes the state and updates the timer.
    /// </summary>
    /// <param name="newState">The new state.</param>
    public virtual void ChangeState(AIState newState)
    {
        //Change the current state
        currentState = newState;

        lastStateChangeTime = Time.time;
    }


    #region States
    /// <summary>
    /// The enemy pauses and checks left and right for the player, then starts to wander again.
    /// </summary>
    protected virtual void DoScanState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }

        if (Time.time >= lastStateChangeTime + 8.0f)
        {
            ChangeState(AIState.Wander);
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


    protected void DoChaseState()
    {
        //The tank checks to see if it should flee.
        if (pawn.health.currentHealth / pawn.health.maxHealth <= fleeThreshold)
        {
            ChangeState(AIState.Flee);
        }
        else if (CanSee(target.GetComponent<TankMover>().turret))
        {
            lastStateChangeTime = Time.time; //Tells the timer when the last time the enemy saw the player was.
            if (DistanceFromTarget() > followDistance)
            {
                //If the distance between the target and the enemy is too big, then the enemy will move towards the player.
                Seek(target);
            }
            else
            {
                //Stops the enemy from getting too close to the player willingly.
                if (pawn.mover.IsMoving())
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
        else if (Time.time <= lastStateChangeTime + 1)
        {
            //Will continue tracking the player for 1 second after loosing sight.
            Seek(target);
            pawn.mover.TurretRotateTowards(target.transform.position, fastTurretMoveSpeed);

        }
        else if (Vector3.Distance(lastTargetLocation, target.transform.position) < 5)
        {
            //The player is close enough to the lastTargetLocation, he is probably behind cover and not running away.
            pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);
        }
        else if (Time.time >= lastStateChangeTime + searchDelay)
        {
            //Tells the AI to move to the player's last known location if it has gone searchDelay seconds without seeing the player.
            ChangeState(AIState.Search);
        }
        else
        {

        }
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

    /// <summary>
    /// Finds the closest health powerup and navigates towards it.
    /// </summary>
    protected void StartFleeState()
    {
        NavigateToClosestHealthPowerup();
    }

    /// <summary>
    /// Manages when the tank should leave the flee state.
    /// </summary>
    protected virtual void DoFleeState()
    {
        if (pawn.health.currentHealth / pawn.health.maxHealth > fleeThreshold)
        {
            if (CanSeeNoFOV(target.GetComponent<TankMover>().turret)) //Checks to see if the player was chasing this pawn to the health pickup. If so, then it resumes the chase.
            {
                ChangeState(AIState.Chase);
            }
            else
            {
                ChangeState(AIState.Scan);
            }
        }
        else if (Vector2.Distance(new Vector2(targetLocation.x, targetLocation.z), new Vector2(transform.position.x, transform.position.z)) <= 0.1)
        {
            NavigateToClosestHealthPowerup(); //The health is still too low after getting a health pickup, then it navigates to the next one.
        }

        //If the player is chasing the tank while fleeing, the tank will still shoot back.
        if (CanSeeNoFOV(target.GetComponent<TankMover>().turret))
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

    protected virtual void DoAlertState()
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

    /// <summary>
    /// A sub-FSM for the movement states that controlls where the tank turret is looking.
    /// </summary>
    protected void DoLookSubstate()
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
                    if (Time.time > turretTimer)
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
            StartLookSubstate();
        }
    }

    /// <summary>
    /// Decides which Wander sub-state is active and for how long.
    /// </summary>
    protected void StartLookSubstate()
    {
        //Randomizes how long the turret will stay in a sub-state.
        endLookTime = Time.time + Random.Range(minLookTime, maxLookTime);

        //Gets the total weight values for the states.
        float totalWeight = 0;
        foreach (float weight in lookStateWeights)
        {
            totalWeight += weight;
        }

        //Chooses a random float in the range of the weights
        float random = Random.Range(0, totalWeight);

        //Checks to see which state fits the random number chosen.
        float weightIncrement = 0;
        for (int i = 0; i < lookStateWeights.Length; i++)
        {
            if (weightIncrement <= random && weightIncrement + lookStateWeights[i] >= random)
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
                Debug.LogWarning("Patrol Tank's Wander state had its sub-state go out of bounds.");
                break;

        }
    }

    #endregion States

    #region Behaviors
    #region Seek
    public void Seek(GameObject target)
    {
        //The pawn rotates in the direction it's moving.
        pawn.mover.MoveTo(target.transform.position);
    }

    public void Seek(Transform targetTransform)
    {
        //The pawn rotates in the direction it's moving.
        pawn.mover.MoveTo(targetTransform.position);
    }

    public void Seek(Pawn targetPawn)
    {
        //The pawn rotates in the direction it's moving.
        pawn.mover.MoveTo(targetPawn.gameObject.transform.position);
    }

    /// <summary>
    /// Unlike the other overloaded Seek functions, this one sets the target to be a fixed point instead of a GameObject.
    /// </summary>
    /// <param name="targetLocation">The position the pawn will move to.</param>
    public void Seek(Vector3 _targetLocation)
    {
        //The pawn rotates in the direction it's moving.
        targetLocation = _targetLocation;
        pawn.mover.MoveTo(targetLocation);
    }
    #endregion Seek

    protected void NavigateToClosestHealthPowerup()
    {
        if (GameManager.instance.pickups.Count > 0)
        {
            Pickup closestPickup = GameManager.instance.pickups[0];
            float closestDistance = 10000; //Initialize at a big number.

            foreach (Pickup pickup in GameManager.instance.pickups)
            {
                float tempDistance = Vector3.Distance(pickup.transform.position, pawn.transform.position);
                if (tempDistance < closestDistance && pickup.powerup == Powerups.powerupList.Health && pickup.isCollectable)
                {
                    closestPickup = pickup;
                    closestDistance = tempDistance;
                }
            }

            targetLocation = closestPickup.transform.position;
            Seek(targetLocation);
        }
    }

    #endregion Behaviors

    #region Senses
    /// <summary>
    /// Tells the AI controller the player fired a shot near them. 
    /// </summary>
    public virtual void HeardPlayerShoot(Vector3 playerPosition) //This is called in TankShooter whenever the player fires.
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

    /// <summary>
    /// Checks to see if the AI controller is able to see its target.
    /// </summary>
    /// <param name="_target">The GameObject this AI checks if it can see. If passing in the player, pass in the turret from the TankMover script, because the player's position is too low to the ground. </param>
    /// <returns>Returns true if the target is in the AI controller's field of view, or returns the output of the last time time this function was called if it was called less than sightCacheDelay seconds ago.</returns>
    public virtual bool CanSee(GameObject _target)
    {
        if (Time.time > sightCacheTimer + sightCacheDelay)
        {
            // Find the vector from the AI to the target.
            Vector3 aiToTarget = _target.transform.position - pawn.mover.turret.transform.position;
            // Find the angle between the direction our AI is facing and the vector to the target in 2D space.
            float angleToTargetFromBody = Vector2.Angle(new Vector2(aiToTarget.x, aiToTarget.z), new Vector2(pawn.mover.body.transform.forward.x, pawn.mover.body.transform.forward.z)); 
            float angleToTargetFromTurret = Vector2.Angle(new Vector2(aiToTarget.x, aiToTarget.z), new Vector2(pawn.mover.turret.transform.forward.x, pawn.mover.turret.transform.forward.z)); //Need to have the turret angle seperate from the body, otherwise stuff gets weird.

            if (debugMode)
            {
                for (int i = 0; i < 20; i++)
                {
                    Debug.DrawLine(
                        new Vector3(
                            3 * Mathf.Sin((bodyFieldOfView * (i - 10) / 10 + pawn.mover.body.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.body.transform.position.x,
                            0.5f,
                            3 * Mathf.Cos((bodyFieldOfView * (i - 10) / 10 + pawn.mover.body.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.body.transform.position.z),
                        new Vector3(
                            3 * Mathf.Sin((bodyFieldOfView * (i - 9) / 10 + pawn.mover.body.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.body.transform.position.x,
                            0.5f,
                            3 * Mathf.Cos((bodyFieldOfView * (i - 9) / 10 + pawn.mover.body.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.body.transform.position.z),
                        Color.red, sightCacheDelay);
                    Debug.DrawLine(
                        new Vector3(
                            4 * Mathf.Sin((turretFieldOfView * (i - 10) / 10 + pawn.mover.turret.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.turret.transform.position.x,
                            0.5f,
                            4 * Mathf.Cos((turretFieldOfView * (i - 10) / 10 + pawn.mover.turret.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.turret.transform.position.z),
                        new Vector3(
                            4 * Mathf.Sin((turretFieldOfView * (i - 9) / 10 + pawn.mover.turret.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.turret.transform.position.x,
                            0.5f,
                            4 * Mathf.Cos((turretFieldOfView * (i - 9) / 10 + pawn.mover.turret.transform.eulerAngles.y) * Mathf.PI / 180) + pawn.mover.turret.transform.position.z),
                        Color.red, sightCacheDelay);
                }

                RaycastHit hit;
                Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget, out hit);
                Debug.DrawLine(pawn.mover.turret.transform.position, pawn.mover.turret.transform.position + (aiToTarget.normalized * hit.distance), Color.red, sightCacheDelay);
            }

            //Debug.Log("Outside FOV: " + (angleToTargetFromTurret > turretFieldOfView ||
            //    angleToTargetFromTurret < -turretFieldOfView) + "\nangleToTargetFromTurret: " + angleToTargetFromTurret + "\nrotation: " + pawn.mover.turret.transform.eulerAngles.y);

            //Checks to see if the player is outside the field of view.
            if (
                (angleToTargetFromBody > bodyFieldOfView ||
                angleToTargetFromBody < - bodyFieldOfView) &&
                (angleToTargetFromTurret > turretFieldOfView ||
                angleToTargetFromTurret < - turretFieldOfView))
            {
                sightCache = false;
                sightCacheTimer = Time.time;
                return false;
            }
            else
            {
                
                RaycastHit hit;

                //This creates a layer mask that will only collide against Projectiles and Pickups (layers 9 & 10).
                int layerMask = 1 << 9 << 10;
                layerMask = ~layerMask; //This inverses the layermask, meaning the raycast will not collide against projectiles and pickups.

                //If the raycast hits something, then check to see if it's the player.
                //Returns true if the raycast hits a player.
                if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget, out hit, eyesight, layerMask) && hit.transform.gameObject == target)
                {
                    //If it didn't hit a wall, then there must be line of sight for the player.
                    sightCache = true;
                    sightCacheTimer = Time.time;
                    lastTargetLocation = _target.transform.position; //Saves the position of the player.
                    return true;
                }
                else
                {
                    Vector3 perpendicular = new Vector3(-1, aiToTarget.y, aiToTarget.x / aiToTarget.z); //This gets the perpendicular vector to the line between the AI and the player.

                    //Adds the perpendicular of the line with a magnitude equal to the radius of the enemy to the line between the player and the AI. this makes it so the line is from the AI to the left and right sides of the player that the AI is facing.
                    //Although the collider is a square, this math acts like its a sphere. This works since the actual collider is larger.
                    RaycastHit hit2;

                    if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * 1.1f, out hit, eyesight, layerMask) && hit.transform.gameObject == target ||
                        Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * -1.1f, out hit2, eyesight, layerMask) && hit2.transform.gameObject == target)
                    {
                        sightCache = true;
                        sightCacheTimer = Time.time;
                        lastTargetLocation = _target.transform.position; //Saves the position of the player
                        return true; //One of the raycasts hit the player. That means the player is actively peeking around a corner. This prevents the player from aranging themselves to somehow shoot the enemy while the AI thinks there behind a wall.
                    }
                    else
                    {
                        sightCache = false;
                        sightCacheTimer = Time.time;
                        return false; //The raycast did not hit the player.
                    }
                }
            }
        }
        else
            return (sightCache); //Returns the last check if this function was called in the past half second.
    }


    /// <summary>
    /// Checks to see if the AI controller is able to see its target, ignoring FOV restrictions
    /// </summary>
    /// <param name="_target">The GameObject this AI checks if it can see. If passing in the player, pass in the turret from the TankMover script, because the player's position is too low to the ground. </param>
    /// <returns>Returns true if the target is in the AI controller's line of sight, or returns the output of the last time time this function was called if it was called less than sightCacheDelay seconds ago.</returns>
    public bool CanSeeNoFOV(GameObject _target)
    {
        Vector3 aiToTarget = _target.transform.position - pawn.mover.turret.transform.position;

        RaycastHit hit;

        //This creates a layer mask that will only collide against Projectiles and Pickups (layers 9 & 10).
        int layerMask = 1 << 9 << 10;
        layerMask = ~layerMask; //This inverses the layermask, meaning the raycast will not collide against projectiles and pickups.

        //If the raycast hits something, then check to see if it's the player.
        //Returns true if the raycast hits a player.
        if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget, out hit, eyesight, layerMask) && hit.transform.gameObject == target)
        {
            //If it didn't hit a wall, then there must be line of sight for the player.
            sightCache = true;
            sightCacheTimer = Time.time;
            lastTargetLocation = _target.transform.position; //Saves the position of the player.
            return true;
        }
        else
        {
            Vector3 perpendicular = new Vector3(-1, aiToTarget.y, aiToTarget.x / aiToTarget.z); //This gets the perpendicular vector to the line between the AI and the player.

            //Adds the perpendicular of the line with a magnitude equal to the radius of the enemy to the line between the player and the AI. this makes it so the line is from the AI to the left and right sides of the player that the AI is facing.
            //Although the collider is a square, this math acts like its a sphere. This works since the actual collider is larger.
            RaycastHit hit2;

            if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * 1.1f, out hit, eyesight, layerMask) && hit.transform.gameObject == target ||
                Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * -1.1f, out hit2, eyesight, layerMask) && hit2.transform.gameObject == target)
            {
                sightCache = true;
                sightCacheTimer = Time.time;
                lastTargetLocation = _target.transform.position; //Saves the position of the player
                return true; //One of the raycasts hit the player. That means the player is actively peeking around a corner. This prevents the player from aranging themselves to somehow shoot the enemy while the AI thinks there behind a wall.
            }
            else
            {
                sightCache = false;
                sightCacheTimer = Time.time;
                return false; //The raycast did not hit the player.
            }
        }
    }

    public float DistanceFromTarget()
    {
        return(Vector3.Distance(target.transform.position, transform.position));
    }
    public float DistanceFromTarget(Vector3 _target)
    {
        return(Vector3.Distance(_target, transform.position));
    }
    public float DistanceFromTarget(GameObject _target)
    {
        return(Vector3.Distance(_target.transform.position, transform.position));
    }
    
    #endregion Senses
}
