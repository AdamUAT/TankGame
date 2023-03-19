using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    //The powerup this pickup gives
    [SerializeField]
    private Powerups.powerupList powerup;

    [SerializeField]
    [Tooltip("The odds of each powerup being chosen to respawn during the game. The initial powerup is decided by the map generation.")]
    private float[] powerupWeights; //This must be manually set in the prefab, it also must have the same size as the enum.

    [SerializeField]
    [Tooltip("Tells if the pickup has a powerup ready to be collected")]
    private bool isCollectable;

    [SerializeField]
    private GameObject powerupMesh;
    [SerializeField]
    private GameObject powerupParticles;

    [SerializeField]
    [Tooltip("The delay, in seconds, of how long it takes this pickup to respawn a powerup.")]
    float respawnDelay = 10;

    private float nextRespawn;

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
            powerupManager.Add(Powerups.powerups[(int)powerup]);

            //Makes the pickup appear as if it doesn't have a collectable to pick up.
            powerupMesh.SetActive(false);
            powerupParticles.SetActive(false);

            //Calculate when this pickup should respawn it's powerup.
            nextRespawn = Time.time + respawnDelay;

            isCollectable = false;
        }
    }

    private void RespawnPowerup()
    {
        //Gets the total weight values for the powerups.
        float totalWeight = 0;
        foreach (float weight in powerupWeights)
        {
            totalWeight += weight;
        }

        //Chooses a random float in the range of the weights
        float random = Random.Range(0, totalWeight);

        //Checks to see which state fits the random number chosen.
        float weightIncrement = 0;
        for (int i = 0; i < powerupWeights.Length; i++)
        {
            if (weightIncrement <= random && weightIncrement + powerupWeights[i] >= random)
            {
                //Starts some of the LookStates
                switch ((Powerups.powerupList)i)
                {
                    case Powerups.powerupList.Empty:
                        //Does nothing
                        break;
                    case Powerups.powerupList.Health:
                        //Spawns a health powerup.
                        SpawnPowerup(Powerups.powerupList.Health);
                        break;
                    case Powerups.powerupList.Speed:
                        SpawnPowerup(Powerups.powerupList.Speed);
                        break;
                    case Powerups.powerupList.FireRate:
                        SpawnPowerup(Powerups.powerupList.FireRate);
                        break;
                    default:
                        Debug.LogWarning("Offensive Tank's Wander state had its sub-state go out of bounds.");
                        break;

                }
                break; //Stops the rest of the loop from occuring
            }

            weightIncrement += powerupWeights[i];
        }
    }

    /// <summary>
    /// Spawns the powerup visualy
    /// </summary>
    private void SpawnPowerup(Powerups.powerupList powerupToSpawn)
    {
        powerup = powerupToSpawn;

        powerupMesh.GetComponent<Renderer>().material = Powerups.powerups[(int)powerup].powerupMaterial;

        powerupMesh.SetActive(true);
        powerupParticles.SetActive(true);
        
        isCollectable = true;
    }

    private void Update()
    {
        //If the time 
        if(Time.time >= nextRespawn && !isCollectable)
        {
            RespawnPowerup();
        }
    }
}
