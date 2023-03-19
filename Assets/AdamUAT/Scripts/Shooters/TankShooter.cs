using System;
using UnityEditor;
using UnityEngine;

public class TankShooter : Shooter
{
    [SerializeField]
    protected Transform firePointTransform;
    [SerializeField]
    protected GameObject shellPrefab;
    [SerializeField]
    protected float fireForce;
    [SerializeField]
    protected float damageDone;
    [SerializeField]
    protected float shellLifespan;
    [SerializeField]
    [Tooltip("Should the delay be in shots per second or seconds of delay?")]
    private bool isDelayReload = true;
    [SerializeField]
    [Tooltip("The delay between each shot, in seconds.")]
    private float fireRateDelay = 1;
    [SerializeField]
    [Tooltip("How many times the tank can fire in 1 second.")]
    private float fireRate = 1;

    private float reloadCountdown;
    protected bool canFire;

    private void Update()
    {
        if (!canFire)
        {
            reloadCountdown -= Time.deltaTime;
            if (reloadCountdown <= 0)
                Reload();
        }
    }

    /// <summary>
    /// Allows the tank to fire again and calls any methods that happen from that as well.
    /// </summary>
    private void Reload()
    {
        canFire = true;
        //Put anything here that happens when the player can fire again, such as a sound effect.
    }

    /// <summary>
    /// A public method called by the pawn to notify the Shooter to launch a projectile. Also checks to see if the tank has reloaded.
    /// </summary>
    public override void Shoot()
    {
        if (canFire)
        {
            canFire = false;
            //Checks to see which type of delay should be applied.
            if (isDelayReload)
            {
                //Sets the delay to the specified delay.
                reloadCountdown = fireRateDelay;
            }
            else
            {
                //Sets the delay to the inverse of the rate of fire.
                //Checks to see if the tank fires 0 times per second.
                if (fireRate != 0)
                    reloadCountdown = 1 / fireRate;
                else
                    Debug.LogWarning("Custom Error: FireRate cannot be 0.");
            }

            LaunchBullet();
        }
        else
        {
            //Put stuff here that happens when the player tries to fire but can't, like a gun pin click sound effect.
        }
    }

    /// <summary>
    /// Decreases the delay between each shot
    /// </summary>
    /// <param name="percentage">Percentage is represented in decimal form.</param>
    public void FireRateBoost(float percentage)
    {
        fireRateDelay *= 1 - percentage;
        fireRate /= 1- percentage;
    }

    /// <summary>
    /// Virtual method for attacking.
    /// </summary>
    protected virtual void LaunchBullet() { }
}
