using UnityEngine;

public class HealEnemy : Enemy
{
    [SerializeField] private float healAmount = 20f;

    private Transform playerTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTarget = collision.transform;
            Player playerComp = playerTarget.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.TakeDamage(enterDamage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player playerComp = collision.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.TakeDamage(stayDamage);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerTarget == collision.transform)
                playerTarget = null;
        }
    }

    protected override void Die()
    {
        HealPlayer(); // Heal player before dying
        base.Die();
    }

    private void HealPlayer()
    {
        if (playerTarget != null)
        {
            Player playerComp = playerTarget.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.Heal(healAmount);
            }
        }
    }
}
