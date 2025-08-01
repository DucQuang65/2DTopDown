using UnityEngine;

public class Lancer : Enemy
{
    private bool hasAttacked = false;

    protected override void HandleAnimation(float distanceToPlayer)
    {
        if (GamePauseManager.IsPaused) return;

        if (animator != null)
        {
            animator.SetBool("IsRun", currentState == EnemyState.ChasingPlayer || currentState == EnemyState.ChasingTower);

            if (distanceToPlayer <= attackRange)
            {
                animator.SetTrigger("Attack");

                // Gây sát thương chỉ khi chưa tấn công (để tránh gây damage liên tục)
                if (!hasAttacked)
                {
                    DamageToPlayer();
                    hasAttacked = true;
                }
            }
            else
            {
                ResetAttack();
            }
        }
    }

    protected virtual void DamageToPlayer()
    {
        if (playerTarget != null)
        {
            PlayerController playerScript = playerTarget.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(enterDamage);
            }
            else
            {
                Debug.LogWarning("Player script not found on target tagged 'Player'.");
            }
        }
    }

    /// Reset the attack flag so the enemy can attack again next time
    protected void ResetAttack()
    {
        hasAttacked = false;
    }
}
