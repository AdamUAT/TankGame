using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateClockwiseKey;
    public KeyCode rotateCounterClockwiseKey;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        ProcessInputs();
    }

    public void ProcessInputs()
    {
        if(Input.GetKey(moveForwardKey))
        {
            pawn.MoveForward();
        }
        
        if(Input.GetKey(moveBackwardKey))
        {
            pawn.MoveBackward();
        }

        if(Input.GetKey(rotateClockwiseKey))
        {
            pawn.RotateClockwise();
        }

        if(Input.GetKey(rotateCounterClockwiseKey))
        {
            pawn.RotateCounterClockwise();
        }
    }
}
