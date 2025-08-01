using UnityEngine;

public class HarpoonFish : Enemy
{
    private bool hasAttacked = false;
    private float damageInterval = 0.5f;
    private float damageTimer = 0f;

    protected override void Update()
    {
        if (GamePauseManager.IsPaused) return;

        UpdateChaseTarget();

        base.Update();

        damageTimer += Time.deltaTime;

        float distanceToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;
        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;

        bool isStandingStill = IsStandingStill();

        bool nearPlayer = currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange && isStandingStill;
        bool nearTower = currentState == EnemyState.ChasingTower && distanceToTower <= attackRange && isStandingStill;

        if (damageTimer >= damageInterval)
        {
            if (!hasAttacked)
            {
                if (nearPlayer)
                {
                    DamageToPlayer(enterDamage);
                    hasAttacked = true;
                }
                else if (nearTower)
                {
                    DamageToTower(enterDamage);
                    hasAttacked = true;
                }
            }
            else
            {
                hasAttacked = false;
            }
            damageTimer = 0f;
        }
    }

    private void UpdateChaseTarget()
    {
        // Ưu tiên tấn công Player nếu còn sống
        if (playerTarget != null)
        {
            PlayerController playerComp = playerTarget.GetComponent<PlayerController>();
            if (playerComp != null && !playerComp.IsDead && playerComp.currentHp > 0)
            {
                currentState = EnemyState.ChasingPlayer;
                return;
            }
            else
            {
                playerTarget = null;
            }
        }

        // Nếu không có Player hoặc Player chết thì tấn công Tower nếu còn sống
        if (tower != null && tower.GetCurrentHP() > 0)
        {
            currentState = EnemyState.ChasingTower;
            return;
        }

        // Không có target thì đứng im
        currentState = EnemyState.Idle;
    }

    private bool IsStandingStill()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        return rb != null ? rb.linearVelocity.sqrMagnitude < 0.01f : true;
    }

    protected void DamageToPlayer(float damageAmount)
    {
        if (playerTarget == null) return;

        PlayerController playerComp = playerTarget.GetComponent<PlayerController>();
        if (playerComp != null)
        {
            playerComp.TakeDamage(damageAmount);
            Debug.Log($"HarpoonFish dealt {damageAmount} damage to Player.");
        }
    }

    protected void DamageToTower(float damageAmount)
    {
        if (tower == null) return;

        tower.TakeDamage(damageAmount);
        Debug.Log($"HarpoonFish dealt {damageAmount} damage to Tower.");
    }

    // Ghi đè để tránh gọi sai trong base class
    protected override void DamageToPlayer() { }
    protected override void DamageToTower() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTarget = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerTarget == collision.transform)
        {
            playerTarget = null;
        }
    }
}
