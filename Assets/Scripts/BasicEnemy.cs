using UnityEngine;

public class BasicEnemy : Enemy
{
    private bool hasAttacked = false;

    protected override void DamageToPlayer()
    {
        if (playerTarget != null && !hasAttacked)
        {
            Player player = playerTarget.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(enterDamage);
                hasAttacked = true;
            }
        }
    }

    protected void ResetAttack()
    {
        hasAttacked = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTarget = collision.transform; // Dùng biến kế thừa từ Enemy
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform == playerTarget)
        {
            playerTarget = null;
            ResetAttack();
        }
    }
}
