using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPawn : Pawn
{
    [SerializeField]
    protected NavMeshAgent navMeshAgent; //A reference to the navMeshAgent of the tank.

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
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
        
    }
}
