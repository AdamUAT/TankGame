using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMover : TankMover
{
    private NavMeshAgent navMeshAgent; //A reference to the navMeshAgent of the tank.

    protected override void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        base.Start();
    }

    /// <summary>
    /// Tells the NavMeshAgent of this pawn to move to a location.
    /// </summary>
    /// <param name="target">The location to move to.</param>
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

    /// <summary>
    /// Determines if the NavMeshAgent is trying to move.
    /// </summary>
    /// <returns>True if the NavMeshAgent is moving.</returns>
    public override bool IsMoving()
    {
        if (navMeshAgent != null)
            return (navMeshAgent.hasPath || navMeshAgent.pathPending);
        else
            return false;
    }

    public override void SpeedBoost(float percentage)
    {
        base.SpeedBoost(percentage);

        navMeshAgent.speed += navMeshAgent.speed * percentage;
        navMeshAgent.acceleration += navMeshAgent.acceleration * percentage;
        navMeshAgent.angularSpeed += navMeshAgent.angularSpeed * percentage;
    }
}
