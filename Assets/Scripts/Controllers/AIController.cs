using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    public enum AIState { Idle, Guard, Chase, Flee, Patrol, Attack, Scan, BackToPost}
    public AIState currentState;
    protected float lastStateChangeTime;
    public GameObject target;

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
}
