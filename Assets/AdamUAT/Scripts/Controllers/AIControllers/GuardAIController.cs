using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAIController : AIController
{
    [SerializeField]
    [Tooltip("Half the angle of the arc the turret will make, in degrees.")]
    protected float scanAmount = 30;
    [SerializeField]
    [Tooltip("How long it takes for the guard to scan from one side to another, in seconds.")]
    protected float scanPeriod = 1.5f;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Initial state is scan.
        ChangeState(AIState.Scan);
    }

    /// <summary>
    /// Calls the behavior that matches the current state.
    /// </summary>
    public override void MakeDecisions()
    {
        switch (currentState)
        {
            case AIState.Scan:
                DoScanState();
                break;
            case AIState.Alert:
                DoAlertState();
                break;
            case AIState.Attack:
                DoAttackState();
                break;
            case AIState.Idle:
                //A default state that doesn't do anything.
                break;
            default:
                Debug.LogWarning("Patrol Tank had its state go out of bounds.");
                break;
        }
    }

    public override void ChangeState(AIState newState)
    {
        //Calls the end states.
        switch (currentState)
        {
            case AIState.Scan:
                break;
            case AIState.Alert:
                break;
            case AIState.Attack:
                break;
            case AIState.Idle:
                //Do nothing, since Idle is a placeholder state.
                break;
            default:
                Debug.LogWarning("Guard Tank had its state go out of bounds.");
                break;
        }

        base.ChangeState(newState); //Changes the state and updates the timer.

        //Calls the begin states.
        switch (newState)
        {
            case AIState.Scan:
                break;
            case AIState.Alert:
                break;
            case AIState.Attack:
                break;
            case AIState.Idle:
                break;
            default:
                Debug.LogWarning("Guard Tank had its state go out of bounds.");
                break;
        }
    }

    protected override void DoScanState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Attack);
        }

        //Oscillates the turret.
        pawn.mover.TurretRotateAngle(scanAmount * Mathf.Sin(Time.time / scanPeriod * Mathf.PI * 2), fastTurretMoveSpeed);
    }

    protected override void DoAlertState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            ChangeState(AIState.Chase);
        }

        //Rotates the turret towards the location the player fired from.
        if (Time.time <= lastStateChangeTime + 1.5f && Time.time >= lastStateChangeTime)
        {
            pawn.mover.TurretRotateTowards(lastTargetLocation, fastTurretMoveSpeed);
        }
        else
        {
            //After 1.5 seconds, it returns to the scan state.
            ChangeState(AIState.Scan);
        }
    }

    protected void DoAttackState()
    {
        if (CanSee(target.GetComponent<TankMover>().turret))
        {
            lastStateChangeTime = Time.time; //Tells the timer when the last time the enemy saw the player was.

            //Rotates the turret toward the player.
            pawn.mover.TurretRotateTowards(target.transform.position, fastTurretMoveSpeed);

            //If the tank is looking at the target, then it will shoot the target.
            Vector3 aiToTarget = target.transform.position - pawn.mover.turret.transform.position;
            //Makes it so the angle is 2d.
            float angleToTargetFromTurret = Vector2.Angle(new Vector2(aiToTarget.x, aiToTarget.z), new Vector2(pawn.mover.turret.transform.forward.x, pawn.mover.turret.transform.forward.z)); //Need to have the turret angle seperate from the body, otherwise stuff gets weird.
            //Allows a degree margin
            if (Mathf.Abs(angleToTargetFromTurret) <= shootFOV)
            {
                pawn.shooter.Shoot();
            }
        }
        else if (Time.time >= lastStateChangeTime + 2)
        {
            //After 2 seconds of loosing sight, it will return to the scan state.
            ChangeState(AIState.Scan);

        }
    }
}
