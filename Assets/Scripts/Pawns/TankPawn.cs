using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPawn : Pawn
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float turnSpeed;

    [SerializeField]
    private GameObject shellPrefab;
    [SerializeField]
    private float fireForce;
    [SerializeField]
    private float damageDone;
    [SerializeField]
    private float shellLifespan;

    private Shooter shooter;
    [SerializeField]
    private float fireRate;
    private float reloadCountdown;
    private bool canFire;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        shooter = GetComponent<Shooter>();
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!canFire)
        {
            reloadCountdown -= Time.deltaTime;
            if (reloadCountdown <= 0)
                Reload();
        }

        base.Update();
    }

    public override void MoveForward()
    {
        if (mover != null)
            mover.Move(transform.forward, moveSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.MoveForward()");
    }

    public override void MoveBackward()
    {
        if (mover != null)
            mover.Move(transform.forward, -moveSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.MoveBackward()");
    }

    public override void RotateClockwise()
    {
        if (mover != null)
            mover.Rotate(turnSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateClockwise()");
    }

    public override void RotateCounterClockwise()
    {
        if (mover != null)
            mover.Rotate(-turnSpeed);
        else
            Debug.LogWarning("Custom Warning: No Mover in TankPawn.RotateCounterClockwise()");
    }

    public override void Shoot()
    {
        if (canFire)
        {
            canFire = false;
            reloadCountdown = fireRate;

            if (shooter != null)
                shooter.Shoot(shellPrefab, fireForce, damageDone, shellLifespan);
            else
                Debug.LogWarning("Custom Warning: No Shooter in TankPawn.Shoot");
        }
        else
        {
            //Put stuff here that happens when the player tries to fire but can't, like a gun pin click sound effect.
        }
    }

    private void Reload()
    {
        canFire = true;
        //Put anything here that happens when the player can fire again, such as a sound effect.
    }
}
