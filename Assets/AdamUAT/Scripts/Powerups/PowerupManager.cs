using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.LowLevel.PlayerLoop;

public class PowerupManager : MonoBehaviour
{
    public List<Powerup> activePowerups = new List<Powerup>();

    /// <summary>
    /// Adds a powerup's effects to this pawn.
    /// </summary>
    /// <param name="powerupToAdd">A copy of the powerup to be added to this pawn.</param>
    public void Add(Powerup powerupToAdd)
    {
        //Apply the powerup's effects
        powerupToAdd.Apply(this);

        //Save it to a list of active powerups if the powerup has a duration.
        if (powerupToAdd.duration > 0)
        {
            powerupToAdd.expiration = Time.time + powerupToAdd.duration;
            activePowerups.Add(powerupToAdd);
        }
    }

    /// <summary>
    /// Removes a powerup's effects from this pawn.
    /// </summary>
    /// <param name="powerupToRemove"></param>
    public void Remove(Powerup powerupToRemove)
    {
        powerupToRemove.Remove(this);
        activePowerups.Remove(powerupToRemove);
    }

    void Update()
    {
        List<Powerup> removedPowerupQueue = new List<Powerup>();

        // One-at-a-time, put each object in "powerups" into the variable "powerup" and do the loop body on it
        foreach (Powerup powerup in activePowerups)
        {
            // If time is up, we want to remove this powerup
            if (Time.time >= powerup.expiration)
            {
                removedPowerupQueue.Add(powerup);
            }
        }
        //Then remove each powerup in the queue
        foreach(Powerup powerup in removedPowerupQueue)
        {
            Remove(powerup);
        }
        //Clear our queue
        removedPowerupQueue.Clear();
    }
}
