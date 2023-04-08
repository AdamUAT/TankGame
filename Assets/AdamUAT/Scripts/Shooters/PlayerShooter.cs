﻿using System;
using UnityEditor;
using UnityEngine;

public class PlayerShooter : TankShooter
{
    [SerializeField]
    private AudioClip reload;
    [SerializeField]
    private AudioClip misfire;

   /// <summary>
   /// Instantiates and applies force to a projectile. Also triggers all enemies to potentially hear the player.
   /// </summary>
    protected override void LaunchBullet()
    {
        //Instantiate our Prefab
        GameObject newShell = Instantiate(shellPrefab, firePointTransform.position, firePointTransform.rotation);

        //Get the DamageOnHit
        DamageOnHit doh = newShell.GetComponent<DamageOnHit>();
        if (doh != null)
        {
            doh.damageDone = damageDone;
            doh.owner = GetComponent<Pawn>();
        }

        //Get the Rigidbody
        Rigidbody rb = newShell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePointTransform.forward * fireForce);
        }

        Destroy(newShell, shellLifespan);

        //This returns all enemies in a 50 unit range.
        foreach (AIController enemy in GameManager.instance.npcs)
        {
            //This component must be on the pawn of the player for this to work.
            enemy.HeardPlayerShoot(gameObject.transform.position);
        }
    }

    /// <summary>
    /// Tells the player they can fire again.
    /// </summary>
    protected override void Reload()
    {
        base.Reload();

        AudioSource audioSource = GetComponent<AudioSource>();
        if(audioSource != null)
        {
            audioSource.PlayOneShot(reload);
        }
    }

    /// <summary>
    /// If the player can't shoot yet, it playes a click sound.
    /// </summary>
    public override void Shoot()
    {
        if (canFire)
        {
            base.Shoot();
        }
        else
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(misfire);
            }
        }
    }

}
