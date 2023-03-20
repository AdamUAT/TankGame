using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;

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

    /// <summary>
    /// Increases the health of the pawn.
    /// </summary>
    /// <param name="amount">The amount to increase the health</param>
    /// <param name="source">The pawn that caused the healing.</param>
    public void Heal(float amount, Pawn source)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void Die(Pawn source)
    {
        AIController controller = GetComponent<AIController>();
        if (controller != null)
        {
            GameManager.instance.npcs.Remove(controller);
        }
        else
        {
            foreach(PlayerController playerController in GameManager.instance.players)
            {
                if(playerController.pawn.gameObject == this.gameObject)
                {
                    playerController.pawn = null;
                    break;
                }
            }
            GameManager.instance.RespawnPlayer();
        }

        Destroy(gameObject);

    }
}
