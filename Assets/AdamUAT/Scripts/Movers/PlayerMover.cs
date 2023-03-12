using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : TankMover
{
    [SerializeField]
    [Tooltip("How fast the tank's turret and camera will rotate compared to the mouse's movements.")]
    private float lookSensitivity;

    protected override void Start()
    {
        base.Start();

        cameraController = GetComponent<CameraController>();
    }

    private void Update()
    {
        //Rotates the turret by the mouse's input.
        TurretRotate(Input.GetAxis("Mouse X") * lookSensitivity);
    }
}
