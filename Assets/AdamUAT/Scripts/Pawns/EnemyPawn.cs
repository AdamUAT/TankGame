using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPawn : Pawn
{
    [SerializeField]
    protected NavMeshAgent navMeshAgent; //A reference to the navMeshAgent of the tank.

    private Shooter shooter; //A reference to the shooter component on this enemy.
    [SerializeField]
    private float fireDelay = 5.0f;
    private float lastShot; //The last time this enemy fired a shot.
    [SerializeField]
    private GameObject shellPrefab;
    [SerializeField]
    private float fireForce;
    [SerializeField]
    private float damageDone;
    [SerializeField]
    private float shellLifespan;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        shooter = GetComponent<Shooter>();

        lastShot = Time.time; //Initializes lastShot.
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void MoveTo(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
    }
    /// <summary>
    /// Cancels the current path for the NavMeshAgent.
    /// </summary>
    public override void StopMoving()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
        }
    }
    public override bool IsMoving()
    {
        if (navMeshAgent != null)
            return (navMeshAgent.hasPath || navMeshAgent.pathPending);
        else
            return false;
    }
    public override void RotateClockwise()
    {
        if (mover != null)
            mover.Rotate(turnSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateClockwise()");
    }

    public override void RotateCounterClockwise()
    {
        if (mover != null)
            mover.Rotate(-turnSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateCounterClockwise()");
    }

    public override void Shoot()
    {
        if (Time.time >= lastShot + fireDelay)
        {
            lastShot = Time.time; //Updates the timer.

            if (shooter != null)
                shooter.Shoot(shellPrefab, fireForce, damageDone, shellLifespan);
            else
                Debug.LogWarning("Custom Warning: No Shooter in EnemyPawn.Shoot");
        }
    }
}
