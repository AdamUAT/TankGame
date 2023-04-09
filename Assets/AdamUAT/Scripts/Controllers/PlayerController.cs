using JetBrains.Annotations;
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
    public KeyCode pauseKey;

    //Lives is stored in the playerController because it is not deleted on the pawn's death.
    public int lives;

    private void Start()
    {
        //Hides and locks the cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lives = 3;
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
        if (GameManager.instance.gameState == GameManager.GameState.GamePlay)
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

                if (Input.GetKeyDown(pauseKey))
                {
                    GameManager.instance.GameStateChange(GameManager.GameState.Pause);

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else
            {
                Debug.LogWarning("Custom Warning: No Mover component found in TankPawn");
            }
        }
        else if (GameManager.instance.gameState == GameManager.GameState.Pause)
        {
            if (Input.GetKeyDown(pauseKey))
            {
                GameManager.instance.GameStateChange(GameManager.GameState.GamePlay);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
