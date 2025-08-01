using UnityEngine;

public class Snake : Enemy
{
    private bool hasAttacked = false;
    private float damageInterval = 0.5f;
    private float damageTimer = 0f;

    private PlayerController playerController;

    protected override void Update()
    {
        if (GamePauseManager.IsPaused) return;

        UpdateChaseTarget();
        base.Update();

        damageTimer += Time.deltaTime;

        if (hasAttacked && damageTimer >= damageInterval)
        {
            hasAttacked = false;
            damageTimer = 0f;
        }

        HandleDamageTick();
    }

    private void UpdateChaseTarget()
    {
        if (tower != null && tower.GetCurrentHP() > 0)
        {
            currentState = EnemyState.ChasingTower;
            return;
        }

        if (playerTarget != null)
        {
            if (playerController == null)
                playerController = playerTarget.GetComponent<PlayerController>();

            if (playerController != null && !playerController.IsDead && playerController.currentHp > 0)
            {
                currentState = EnemyState.ChasingPlayer;
                return;
            }
        }

        currentState = EnemyState.Idle;
    }

    private void HandleDamageTick()
    {
        float distanceToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;
        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;
        bool isStandingStill = IsStandingStill();

        bool nearPlayer = currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange && isStandingStill;
        bool nearTower = currentState == EnemyState.ChasingTower && distanceToTower <= attackRange && isStandingStill;

        if (!hasAttacked)
        {
            if (nearTower)
            {
                DamageToTower(enterDamage);
                hasAttacked = true;
            }
            else if (nearPlayer)
            {
                DamageToPlayer(enterDamage);
                hasAttacked = true;
            }
        }
    }

    private bool IsStandingStill()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        return rb != null ? rb.linearVelocity.sqrMagnitude < 0.01f : true;
    }

    protected override void HandleAnimation(float distanceToPlayer)
    {
        if (GamePauseManager.IsPaused) return;

        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;

        if (animator != null)
        {
            bool isRunning = (currentState == EnemyState.ChasingPlayer && distanceToPlayer > attackRange) ||
                             (currentState == EnemyState.ChasingTower && distanceToTower > attackRange);

            if (HasBoolParameter("IsRun"))
                animator.SetBool(IsRunHash, isRunning);

            if (currentState == EnemyState.ChasingTower && distanceToTower <= attackRange)
            {
                animator.SetTrigger(AttackHash);
                DamageToTower();
            }
            else if (currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange)
            {
                animator.SetTrigger(AttackHash);
                DamageToPlayer();
            }
            else
            {
                ResetAttack();
            }
        }
    }

    protected override void DamageToPlayer()
    {
        if (playerTarget == null || hasAttacked) return;

        if (playerController != null)
        {
            playerController.TakeDamage(enterDamage);
            hasAttacked = true;
        }
    }

    protected void DamageToPlayer(float damageAmount)
    {
        if (playerController != null)
        {
            playerController.TakeDamage(damageAmount);
        }
    }

    protected void DamageToTower(float damageAmount)
    {
        if (tower != null)
        {
            tower.TakeDamage(damageAmount);
        }
    }

    protected override void StayDamageToPlayer()
    {
        if (playerController == null) return;

        stayDamageTimer += Time.deltaTime;
        if (stayDamageTimer >= stayDamageInterval)
        {
            playerController.TakeDamage(stayDamage);
            stayDamageTimer = 0f;
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
            playerTarget = collision.transform;
            playerController = collision.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerTarget == collision.transform)
        {
            playerTarget = null;
            playerController = null;
        }
    }
}
