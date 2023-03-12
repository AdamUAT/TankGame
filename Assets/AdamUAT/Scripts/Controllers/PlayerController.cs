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

    private void Start()
    {
        //Hides and locks the cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ProcessInputs();
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
        if (pawn.mover != null)
        {
            if (Input.GetKey(rotateClockwiseKey))
            {
                pawn.mover.BodyRotate(true);
            }

            if (Input.GetKey(rotateCounterClockwiseKey))
            {
                pawn.mover.BodyRotate(false);
            }

            if (Input.GetKeyDown(shootKey))
            {
                pawn.shooter.Shoot();
            }

            if (Input.GetKey(moveForwardKey))
            {
                pawn.mover.Move(true); ;
            }

            if (Input.GetKey(moveBackwardKey))
            {
                pawn.mover.Move(false);
            }
        }
        else
        {
            Debug.LogWarning("Custom Warning: No Mover component found in TankPawn");
        }
    }
}
