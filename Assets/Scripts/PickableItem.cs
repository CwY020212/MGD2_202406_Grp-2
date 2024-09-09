using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private int scoreValue = 10; // The score value of the collectible

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Item collected: {itemName}");

            // Get the PlayerBehavior script from the player
            PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();

            if (playerBehavior != null)
            {
                // Update the player's score
                playerBehavior.Score += scoreValue;

                // Optional: Perform additional actions when the item is collected
                // e.g., play a sound, trigger an animation, etc.

                // Destroy the collectible item
                Destroy(gameObject);
            }
        }
    }
}
