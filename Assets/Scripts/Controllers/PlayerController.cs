using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Controller
{
    public KeyCode moveForwardKey;
    public KeyCode moveBackwardKey;
    public KeyCode rotateClockwiseKey;
    public KeyCode rotateCounterClockwiseKey;
    public KeyCode shootKey;

    // Start is called before the first frame update
    public override void Start()
    {
        // If we have a GameManager
        if (GameManager.instance != null)
        {
            // And it tracks the player(s)
            if (GameManager.instance.players != null)
            {
                // Register with the GameManager
                GameManager.instance.players.Add(this);
            }
        }

        // Run the Start() function from the parent (base) class
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        ProcessInputs();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        ProcessLateInputs();
    }

    public void OnDestroy()
    {
        if(GameManager.instance != null)
        {
            if(GameManager.instance.players != null)
            {
                GameManager.instance.players.Remove(this);
            }
        }
    }

    /// <summary>
    /// Checks to see if the player put in any inputs, and then doesn non-physics-based effects based on those inputs.
    /// </summary>
    public void ProcessInputs()
    {
        if(Input.GetKey(rotateClockwiseKey))
        {
            pawn.RotateClockwise();
        }

        if(Input.GetKey(rotateCounterClockwiseKey))
        {
            pawn.RotateCounterClockwise();
        }

        if(Input.GetKeyDown(shootKey))
        {
            pawn.Shoot();
        }
    }
    /// <summary>
    /// Checks to see if the player put in any inputs, and then does physics-based effects based on those inputs.
    /// </summary>
    public void ProcessLateInputs()
    {
        if (Input.GetKey(moveForwardKey))
        {
            pawn.MoveForward();
        }

        if (Input.GetKey(moveBackwardKey))
        {
            pawn.MoveBackward();
        }
    }
}
