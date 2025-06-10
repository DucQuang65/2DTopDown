using UnityEngine;

public class BasicEnemy : Enemy// Inherits from the Enemy class
{
    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                player.TakeDamage(enterDamage); // Apply initial damage when entering the trigger
            }
        }
    }
    private void OnTriggerStay2D(UnityEngine.Collider2D collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            if (player != null)
            {
                player.TakeDamage(stayDamage); // Apply damage over time while staying in the trigger
            }
        }
    }
}
