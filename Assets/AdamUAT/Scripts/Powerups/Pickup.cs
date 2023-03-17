using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    //The powerup this pickup gives
    [SerializeField]
    private Powerups.powerupList powerup;

    [SerializeField]
    [Tooltip("Tells if the pickup has a powerup ready to be collected")]
    private bool isCollectable;

    [SerializeField]
    private GameObject powerupMesh;
    [SerializeField]
    private GameObject powerupParticles;

    void Start()
    {
        //Accesses the material from the list of powerups.
        powerupMesh.GetComponent<Renderer>().material = Powerups.powerups[(int)powerup].powerupMaterial;
    }

    public void OnTriggerEnter(Collider other)
    {
        // variable to store other object's PowerupController - if it has one
        PowerupManager powerupManager = other.GetComponent<PowerupManager>();

        // If the other object has a PowerupController
        if (powerupManager != null && isCollectable)
        {
            // Add the powerup
            //powerupManager.Add(powerup);

            //Makes the pickup appear as if it doesn't have a collectable to pick up.
            powerupMesh.SetActive(false);
            powerupParticles.SetActive(false);
        }
    }
}
