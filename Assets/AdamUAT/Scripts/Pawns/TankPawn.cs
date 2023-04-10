using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    [HideInInspector]
    public TankShooter shooter;
    [HideInInspector]
    public CameraController cameraController;

    protected override void Start()
    {
        shooter = GetComponent<TankShooter>();
        cameraController = GetComponent<CameraController>();
        base.Start();
    }
}
