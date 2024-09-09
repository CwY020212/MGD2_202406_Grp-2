using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerBehavior;

public class PowerUp : MonoBehaviour
{
    public PlayerBehavior.PowerUpType type; // Enum type from PlayerBehavior

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the PlayerBehavior script from the player
            PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();

            if (playerBehavior != null)
            {
                // Apply the power-up effect to the player
                playerBehavior.ApplyPowerUp(type);

                // Optionally, send a message to perform an operation
                SendMessage("Operate", SendMessageOptions.DontRequireReceiver);

                // Destroy the power-up object
                Destroy(gameObject);
            }
        }
    }
}
