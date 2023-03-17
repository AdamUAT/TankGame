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
    public enum powerupList {Empty, Health};

    #region PowerupMaterials
    [SerializeField]
    private Material defaultPowerup;
    [SerializeField]
    private Material healthPowerup;
    #endregion PowerupMaterials

    void Start()
    {
        //Populate the static list of powerups.
        if (powerups.Count == 0)
        {
            Powerup powerup;

            //Powerups must be assigned in the order they appear in the enum.

            powerup = new Powerup();
            powerup.powerupMaterial = defaultPowerup;
            powerups.Add(powerup);

            powerup = new HealthPowerup();
            powerup.powerupMaterial = healthPowerup;
            powerups.Add(powerup);
        }
    }
}

public class Powerup
{
    public Material powerupMaterial;
    public virtual void Apply(PowerupManager target) { }
    public virtual void Remove(PowerupManager target) { }
}

public class HealthPowerup : Powerup
{
    public override void Apply(PowerupManager target)
    {

    }

    public override void Remove(PowerupManager target)
    {

    }
}
