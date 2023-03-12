using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    [HideInInspector]
    public TankShooter shooter;
    [HideInInspector]
    public TankMover mover;
    private CameraController cameraController;


    // Start is called before the first frame update
    private void Start()
    {
        shooter = GetComponent<TankShooter>();
        mover = GetComponent<TankMover>();
        cameraController = GetComponent<CameraController>();
    }

    //This update is only for cameras, as it helps stop making things jittery.
    public void LateUpdate()
    {
        cameraController.UpdateCameraPosition(); //Moves the camera smoothly towards where it should be.
    }

    public override void MoveForward()
    {
        if (mover != null)
            mover.Move(true);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.MoveForward()");
    }

    public override void MoveBackward()
    {
        if (mover != null)
            mover.Move(false);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.MoveBackward()");
    }

    public override void RotateClockwise()
    {
        if (mover != null)
            mover.BodyRotate(true);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateClockwise()");
    }

    public override void RotateCounterClockwise()
    {
        if (mover != null)
            mover.BodyRotate(false);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateCounterClockwise()");
    }
    public override void Shoot()
    {
        if (shooter != null)
            shooter.Shoot();
        else
            Debug.LogWarning("Custom Warning: No Shooter in TankPawn.Shoot");
    }
}
