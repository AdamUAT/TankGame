using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : Controller
{
    public enum AIState { Idle, Guard, Chase, Flee, Patrol, Attack, Scan, BackToPost, Wander, Search}
    public AIState currentState;
    protected float lastStateChangeTime;
    public GameObject target; //This IS NOT equivilant to the player. This is any gameobject the AI is set to move to.
    protected Vector3 targetLocation; //This is similiar to target, but is a set location instead of a moving GameObject.

    private bool sightCache; //This holds the past result of CanSee.
    private float sightCacheTimer; //A timer that controlls when CanSee will recaculate vs. return the previous calculation.
    [SerializeField]
    private float sightCacheDelay = 0.5f; //The variable that controlls how often this AI checks for vision.

    //hearingRange needs to be accessed by TankShooter.
    public float hearingRange = 20.0f;
    [SerializeField]
    protected float fieldOfView = 45.0f; //The angle of half of the AI's field of view, in degrees.
    [SerializeField]
    protected float eyesight = 15f; //The range the tanks are able to see the player.

    [SerializeField]
    protected NavMeshSurface navMesh; //A reference to the NavMesh this AI uses.

    public override void Start()
    {
        currentState = AIState.Idle;

        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        CheckSenses();
        MakeDecisions();

        base.Update();
    }

    public virtual void MakeDecisions()
    {

    }

    public virtual void CheckSenses()
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
    public void DoSeekState()
    {

    }

    public virtual void DoIdleState()
    { 
        
    }
    #endregion States

    #region Behaviors
    #region Seek
    public void Seek(GameObject target)
    {
        //The pawn rotates in the direction it's moving.
        this.target = target;
        pawn.MoveTo(target.transform.position);
    }

    public void Seek(Transform targetTransform)
    {
        //The pawn rotates in the direction it's moving.
        target = targetTransform.gameObject;
        pawn.MoveTo(targetTransform.position);
    }

    public void Seek(Pawn targetPawn)
    {
        //The pawn rotates in the direction it's moving.
        target = targetPawn.gameObject;
        pawn.MoveTo(targetPawn.gameObject.transform.position);
    }

    /// <summary>
    /// Unlike the other overloaded Seek functions, this one sets the target to be a fixed point instead of a GameObject.
    /// </summary>
    /// <param name="targetLocation"></param>
    public void Seek(Vector3 targetLocation)
    {
        //The pawn rotates in the direction it's moving.
        this.targetLocation = targetLocation;
        pawn.MoveTo(targetLocation);
    }
    #endregion Seek

    #endregion Behaviors

    #region Senses
    /// <summary>
    /// Tells the AI controller the player fired a shot near them. 
    /// </summary>
    public virtual void HeardPlayerShoot() //This is called in TankShooter whenever the player fires.
    {

    }
    /// <summary>
    /// Checks to see if the AI controller is able to see its target.
    /// </summary>
    /// <returns>Returns true if the target is in the AI controller's field of view, or returns the output of the last time time this function was called if it was called less than sightCacheDelay seconds ago.</returns>
    public virtual bool CanSee(GameObject _target)
    {
        if (Time.time > sightCacheTimer + sightCacheDelay)
        {
            // Find the vector from the AI to the target
            Vector3 aiToTarget = _target.transform.position - transform.position;
            // Find the angle between the direction our AI is facing (forward in local space) and the vector to the target.
            float angleToTarget = Vector3.Angle(aiToTarget, pawn.transform.forward);
            //Returns false if the player is outside the field of view.
            if (angleToTarget > fieldOfView)
            {
                sightCache = false;
                sightCacheTimer = Time.time;
                return false;
            }
            else
            {
                //This creates a layer mask that will only collide against Walls (layer 11).
                int layerMask = 1 << 11; //Add layer 11 to the mask

                //Returns true if the raycast hits a wall.
                if (Physics.Raycast(transform.position, aiToTarget, eyesight, layerMask))
                {
                    Vector3 perpendicular = new Vector3(-1, aiToTarget.y, aiToTarget.x / aiToTarget.z); //This gets the perpendicular vector to the line between the AI and the player.

                    //Adds the perpendicular of the line with a magnitude equal to the radius of the enemy to the line between the player and the AI. this makes it so the line is from the AI to the left and right sides of the player that the AI is facing.
                    //Although the collider is a square, this math acts like its a sphere. This works since the actual collider is larger.
                    if (Physics.Raycast(transform.position, aiToTarget + perpendicular.normalized * 1.1f, eyesight, layerMask) && Physics.Raycast(transform.position, aiToTarget + perpendicular.normalized * -1.1f, eyesight, layerMask))
                    {
                        sightCache = false;
                        sightCacheTimer = Time.time;
                        return false; //The raycast hit a wall.
                    }
                    else
                    {
                        sightCache = true;
                        sightCacheTimer = Time.time;
                        return true; //One of the raycasts did not hit a wall. That means the player is actively peeking around a corner. This prevents the player from aranging themselves to somehow shoot the enemy while the AI thinks there behind a wall.
                    }
                }
                else
                {
                    //If it didn't hit a wall, then there must be line of sight for the player.
                    sightCache = true;
                    sightCacheTimer = Time.time;
                    return true;
                }
            }
        }
        else
            return (sightCache); //Returns the last check if this function was called in the past half second.
    }
    #endregion Senses
}
