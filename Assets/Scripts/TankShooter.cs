using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooter : Shooter
{
    public Transform firePointTransform;

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public override void Shoot(GameObject shellPrefab, float fireForce, float damageDone, float lifeSpan)
    {
        //Instantiate our Prefab
        GameObject newShell = Instantiate(shellPrefab, firePointTransform.position, firePointTransform.rotation);

        //Get the DamageOnHit
        DamageOnHit doh = newShell.GetComponent<DamageOnHit>();
        if(doh != null)
        {
            doh.damageDone = damageDone;
            doh.owner = GetComponent<Pawn>();
        }

        //Get the Rigidbody
        Rigidbody rb = newShell.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.AddForce(firePointTransform.forward * fireForce);
        }

        Destroy(newShell, lifeSpan);
    }
}
