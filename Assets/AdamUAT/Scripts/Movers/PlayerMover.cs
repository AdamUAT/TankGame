using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : TankMover
{
    [SerializeField]
    [Tooltip("How fast the tank's turret and camera will rotate compared to the mouse's movements.")]
    private float lookSensitivity = 1;

    protected override void Start()
    {
        base.Start();

        cameraController = GetComponent<CameraController>();
    }

    private void Update()
    {
        //Only moves the camera if the gameState is when the player is controlling a tank.
        if (GameManager.instance.gameState == GameManager.GameState.GamePlay)
        {
            //Rotates the turret by the mouse's input.
            TurretRotate(Input.GetAxis("Mouse X") * lookSensitivity);
        }
    }
}
