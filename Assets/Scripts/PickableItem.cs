using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [SerializeField] string itemName;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Item collected: {itemName}");
            Destroy(this.gameObject); // Destroy the item, not the camera
        }

    }
}
