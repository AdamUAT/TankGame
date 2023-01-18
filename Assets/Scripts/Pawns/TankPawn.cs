using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    [SerializeField]
    private float MoveSpeed;
    [SerializeField]
    private float TurnSpeed;


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

    public override void MoveForward()
    {
        Debug.Log("Moving forward");
    }

    public override void MoveBackward()
    {
        Debug.Log("Moving backward");
    }

    public override void RotateClockwise()
    {
        Debug.Log("Rotating clockwise");
    }

    public override void RotateCounterClockwise()
    {
        Debug.Log("Rotating counter clockwise");
    }
}
