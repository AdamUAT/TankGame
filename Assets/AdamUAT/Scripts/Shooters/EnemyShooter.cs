using System;
using UnityEditor;
using UnityEngine;

public class EnemyShooter : TankShooter
{
    /// <summary>
    /// Instantiates and applies force to a projectile.
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
    }

    public void ForceReload()
    {
        canFire = true;
    }
}