using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerups : MonoBehaviour
{
    /// <summary>
    /// A list of every possible powerup.
    /// </summary>
    public static List<Powerup> powerups = new List<Powerup>(); //This is needed because the classes themselve's can't assign the material to itself.
    public enum powerupList {Empty, Health, Speed};

    #region PowerupMaterials
    [SerializeField]
    private Material defaultPowerupMaterial;
    [SerializeField]
    private Material healthPowerupMaterial;
    [SerializeField]
    private Material speedPowerupMaterial;
    #endregion PowerupMaterials

    #region PowerupVariables
    [SerializeField]
    [Tooltip("The amount of healing the HealthPowerup will add.")]
    private float healthPowerupAmount = 20;
    [SerializeField]
    [Tooltip("The percentage of a boost that the powerup gives.")]
    private float speedPowerupAmount = 0.5f;
    [SerializeField]
    [Tooltip("The amount of healing the HealthPowerup will add.")]
    private float healthPowerupDuration = 5;
    #endregion PowerupVariables

    void Start()
    {
        //Populate the static list of powerups.
        if (powerups.Count == 0)
        {
            //Powerups must be assigned in the order they appear in the enum.

            Powerup powerup;
            powerup = new Powerup();
            powerup.powerupMaterial = defaultPowerupMaterial;
            powerup.duration = -1;
            powerups.Add(powerup);

            HealthPowerup healthPowerup;
            healthPowerup = new HealthPowerup();
            healthPowerup.powerupMaterial = healthPowerupMaterial;
            healthPowerup.healthToAdd = healthPowerupAmount;
            powerup.duration = -1;
            powerups.Add(healthPowerup);

            SpeedPowerup speedPowerup = new SpeedPowerup();
            speedPowerup.powerupMaterial = speedPowerupMaterial;
            speedPowerup.duration = 5;
            speedPowerup.speadIncrease = speedPowerupAmount;
            powerups.Add(speedPowerup);
        }
    }
}

public class Powerup
{
    public Material powerupMaterial;
    public float duration; //How long this powerup should be applied.
    public float expiration; //A float used to tell when this powerup should be removed.
    public virtual void Apply(PowerupManager target) { }
    public virtual void Remove(PowerupManager target) { }
    //public virtual void Variables
}

public class HealthPowerup : Powerup
{
    public float healthToAdd;
    public override void Apply(PowerupManager target)
    {
        // Apply Health changes
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            // The second parameter is the pawn who caused the healing - in this case, they healed themselves
            targetHealth.Heal(healthToAdd, target.GetComponent<Pawn>());
        }
    }

    public override void Remove(PowerupManager target)
    {

    }
}

public class SpeedPowerup : Powerup
{
    public float speadIncrease;

    public override void Apply(PowerupManager target)
    {
        TankPawn pawn = target.GetComponent<TankPawn>();
        if(pawn != null)
        {
            //gives a 50% speed boost.
            pawn.mover.SpeedBoost(speadIncrease);
        }
    }

    public override void Remove(PowerupManager target)
    {
        TankPawn pawn = target.GetComponent<TankPawn>();
        if (pawn != null)
        {
            //gives a 50% speed reduction.
            pawn.mover.SpeedBoost(-speadIncrease);
        }
    }
}
