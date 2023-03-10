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
    protected NavMeshSurface navMesh; //A reference to the NavMesh this AI uses.

    [SerializeField]
    [Tooltip("Begins showing debug gizmos for the AI")]
    protected bool debugMode = false;

    protected virtual void Start()
    {
        currentState = AIState.Idle;

        //This is in the AIController class instead of Controller class because the player's controller is spawned on a different GameObject than the player at runtime.
        pawn = GetComponent<TankPawn>();
        GameManager.instance.npcs.Add(this);
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
    /*
    public void DoSeekState()
    {

    }

    public virtual void DoIdleState()
    { 
        
    }*/

    #endregion States

    #region Behaviors
    #region Seek
    public void Seek(GameObject target)
    {
        //The pawn rotates in the direction it's moving.
        this.target = target;
        pawn.mover.MoveTo(target.transform.position);
    }

    public void Seek(Transform targetTransform)
    {
        //The pawn rotates in the direction it's moving.
        target = targetTransform.gameObject;
        pawn.mover.MoveTo(targetTransform.position);
    }

    public void Seek(Pawn targetPawn)
    {
        //The pawn rotates in the direction it's moving.
        target = targetPawn.gameObject;
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

    #endregion Behaviors
    
    #region Senses
    /// <summary>
    /// Tells the AI controller the player fired a shot near them. 
    /// </summary>
    public virtual void HeardPlayerShoot(Vector3 playerPosition) //This is called in TankShooter whenever the player fires.
    {

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
            // Find the angle between the direction our AI is facing (forward in local space) and the vector to the target.
            float angleToTargetFromBody = Vector3.Angle(aiToTarget, pawn.mover.body.transform.forward); 
            float angleToTargetFromTurret = Vector3.Angle(aiToTarget, pawn.mover.turret.transform.forward); //Need to have the turret angle seperate from the body, otherwise stuff gets weird.

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

                //If the raycast hits something, then check to see if it's the player.
                //Returns true if the raycast hits a player.
                if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget, out hit, eyesight) && hit.transform.gameObject == target)
                {
                    //If it didn't hit a wall, then there must be line of sight for the player.
                    sightCache = true;
                    sightCacheTimer = Time.time;
                    lastTargetLocation = _target.transform.position; //Saves the position of the player.
                    Debug.Log("Tank saw the player");
                    return true;
                }
                else
                {
                    Vector3 perpendicular = new Vector3(-1, aiToTarget.y, aiToTarget.x / aiToTarget.z); //This gets the perpendicular vector to the line between the AI and the player.

                    //Adds the perpendicular of the line with a magnitude equal to the radius of the enemy to the line between the player and the AI. this makes it so the line is from the AI to the left and right sides of the player that the AI is facing.
                    //Although the collider is a square, this math acts like its a sphere. This works since the actual collider is larger.
                    RaycastHit hit2;

                    if (Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * 1.1f, out hit, eyesight) && hit.transform.gameObject == target ||
                        Physics.Raycast(pawn.mover.turret.transform.position, aiToTarget + perpendicular.normalized * -1.1f, out hit2, eyesight) && hit2.transform.gameObject == target)
                    {
                        sightCache = true;
                        sightCacheTimer = Time.time;
                        lastTargetLocation = _target.transform.position; //Saves the position of the player
                        Debug.Log("Tank saw the player");
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
