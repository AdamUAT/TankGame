using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    [HideInInspector]
    public TankShooter shooter;

    protected override void Start()
    {
        shooter = GetComponent<TankShooter>();
        base.Start();
    }
}
