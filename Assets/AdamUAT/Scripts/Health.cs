using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

    [HideInInspector]
    public GameObject healthBar;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        
        //Makes sure the pawn is an enemy.
        AIController controller = GetComponent<AIController>();
        if (controller != null)
        {
            healthBar = transform.Find("Canvas").Find("HealthBar").gameObject;
        }
        else
        {
            //Finds the controller that matches this health component.
            foreach(PlayerController playerController in GameManager.instance.players)
            {
                if(playerController.pawn.health == this)
                {
                    healthBar = playerController.hud.gameObject;
                    UpdateHealthBar();
                }
            }
        }
    }

    public void TakeDamage(float amount, Pawn source)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(currentHealth <= 0)
        {
            Die(source);
        }

        UpdateHealthBar();
    }

    /// <summary>
    /// Increases the health of the pawn.
    /// </summary>
    /// <param name="amount">The amount to increase the health</param>
    /// <param name="source">The pawn that caused the healing.</param>
    public void Heal(float amount, Pawn source)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
    }

    public void Die(Pawn source)
    {
        AIController controller = GetComponent<AIController>();
        if (controller != null)
        {
            //Find the player that killed this pawn and give the player score.
            foreach(PlayerController playerController in GameManager.instance.players)
            {
                if(playerController.pawn == source)
                {
                    playerController.IncreaseScore(50);
                }
            }
            GameManager.instance.npcs.Remove(controller);
        }
        else
        {
            foreach(PlayerController playerController in GameManager.instance.players)
            {
                if(playerController.pawn.gameObject == this.gameObject)
                {
                    playerController.pawn = null;

                    //Adjusts the remaining lives of the player.
                    playerController.lives--;
                    healthBar.GetComponent<UI_Object>().UpdateLifeDisplay(playerController.lives);
                    if (playerController.lives < 0)
                    {
                        GameManager.instance.gameState = GameManager.GameState.GameOver;
                    }
                    else
                    {
                        GameManager.instance.RespawnPlayer();
                    }


                    break;
                }
            }
        }

        Destroy(gameObject);

    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            UI_Object ui_Object = healthBar.GetComponent<UI_Object>();

            if (ui_Object != null)
            {
                ui_Object.UpdateHealthBar(currentHealth / maxHealth);
            }
            else
            {
                Slider slider = healthBar.transform.GetComponent<Slider>();
                if(slider != null)
                {
                    slider.value = currentHealth / maxHealth;
                }
            }
        }
    }
}
