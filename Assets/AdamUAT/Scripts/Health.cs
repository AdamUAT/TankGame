﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private float currentHealth;
    [SerializeField]
    private float maxHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Pawn source)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(currentHealth <= 0)
        {
            Die(source);
        }
    }

    public void Die(Pawn source)
    {
        Destroy(gameObject);
    }
}
