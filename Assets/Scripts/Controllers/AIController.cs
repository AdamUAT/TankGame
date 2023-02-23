using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    public enum AIState { Idle, Guard, Chase, Flee, Patrol, Attack, Scan, BackToPost}
    public AIState currentState;
    protected float lastStateChangeTime;
    public GameObject target; //This IS NOT equivilant to the player. This is any gameobject the AI is set to move to.


    [SerializeField]
    protected float hearingRange = 20.0f;
    [SerializeField]
    protected float fieldOfView = 45.0f; //The angle of half of the AI's field of view, in degrees.
    [SerializeField]
    protected float eyesight = 15f; //The range the tanks are able to see the player.

    void Start()
    {
        currentState = AIState.Idle;

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        MakeDecisions();

        base.Update();
    }

    public void MakeDecisions()
    {
        Debug.Log("Making Decisions");
    }

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
    #endregion States

    #region Behaviors
    public void Seek(GameObject target)
    {
        pawn.RotateTowards(target.transform.position);

        pawn.MoveForward();
    }

    public void Seek(Transform targetTransform)
    {
        pawn.RotateTowards(targetTransform.position);

        pawn.MoveForward();
    }

    public void seek(Pawn targetPawn)
    {
        pawn.RotateTowards(targetPawn.transform.position);

        pawn.MoveForward();
    }
    #endregion Behaviors

    #region Senses
    /// <summary>
    /// Tells this AI controller the player fired a shot near them. 
    /// </summary>
    public virtual void HeardPlayerShoot(Vector3 playerPosition) //This is called in TankShooter whenever the player fires.
    {
        //Sets the hearing range of the AI.
        hearingRange = 25;
        //Checks to see if the AI heard the shot.
        if(Vector3.Distance(playerPosition, transform.position) > hearingRange)
        {
            //Do something here such as switching states and behaviors.
        }
    }
    /// <summary>
    /// Checks to see if the AI controller is able to see its target.
    /// </summary>
    /// <returns>Returns true if the target is in the AI controller's field of view.</returns>
    public virtual bool CanSee()
    {
        // Find the vector from the AI to the target
        Vector3 aiToTarget = target.transform.position - transform.position;
        // Find the angle between the direction our AI is facing (forward in local space) and the vector to the target.
        float angleToTarget = Vector3.Angle(aiToTarget, pawn.transform.forward);
        //Returns false if the player is outside the field of view.
        if (angleToTarget > fieldOfView)
        {
            return false;
        }
        else
        {
            //This creates a layer mask that will ignore everything except the Walls (layer 11).
            int layerMask = 1 << 11; //Add layer 11 to mask to ignore
            layerMask = ~layerMask; //This inverts all layers, so layer 11 is the only layer not being ignored.

            //Returns true if the raycast hits a wall.
            if (Physics.Raycast(transform.position, aiToTarget, eyesight, layerMask))
            {
                Vector3 perpendicular = new Vector3(-1, aiToTarget.y, aiToTarget.x / aiToTarget.z); //This gets the perpendicular vector to the line between the AI and the player.

                //Adds the perpendicular of the line with a magnitude equal to the radius of the enemy to the line between the player and the AI. this makes it so the line is from the AI to the left and right sides of the player that the AI is facing.
                //Although the collider is a square, this math acts like its a sphere. This works since the actual collider is larger.
                if(Physics.Raycast(transform.position, aiToTarget + perpendicular.normalized * 1.1f, eyesight, layerMask) || Physics.Raycast(transform.position, aiToTarget + perpendicular.normalized * -1.1f, eyesight, layerMask))
                {
                    return false; //The raycast hit a wall.
                }
                else
                {
                    return true; //One of the raycasts did not hit a wall. That means the player is actively peeking around a corner. This prevents the player from aranging themselves to somehow shoot the enemy while the AI thinks there behind a wall.
                }
            }
            else
            {
                //If it didn't hit a wall, then there must be line of sight for the player.
                return true;
            }
        }
    }
    #endregion Senses
}
