using UnityEngine;

public class Spider : Enemy
{
    private bool hasAttacked = false;
    private float damageInterval = 0.5f;
    private float damageTimer = 0f;

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDestroyDelay = 0.2f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamageToTower = 10f;
    [SerializeField] private float explosionDamageToPlayer = 3f;

    private PlayerController playerController;

    protected override void Update()
    {
        if (GamePauseManager.IsPaused) return;

        UpdateChaseTarget();
        base.Update();

        damageTimer += Time.deltaTime;

        // Khi đã tấn công, đợi damageInterval rồi reset hasAttacked để có thể tấn công tiếp
        if (hasAttacked && damageTimer >= damageInterval)
        {
            hasAttacked = false;
            damageTimer = 0f;
        }

        HandleDamageTick();
    }

    private void UpdateChaseTarget()
    {
        // Ưu tiên tấn công Tower nếu còn sống
        if (tower != null && tower.GetCurrentHP() > 0)
        {
            currentState = EnemyState.ChasingTower;
            return;
        }

        // Nếu không có Tower thì mới xét Player
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

        // Nếu không có mục tiêu nào khả dụng thì đứng im
        currentState = EnemyState.Idle;
    }

    private void HandleDamageTick()
    {
        float distanceToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;
        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;
        bool isStandingStill = IsStandingStill();

        bool nearPlayer = currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange && isStandingStill;
        bool nearTower = currentState == EnemyState.ChasingTower && distanceToTower <= attackRange && isStandingStill;

        // Chỉ tấn công nếu chưa trong cooldown (hasAttacked == false)
        if (!hasAttacked)
        {
            if (nearTower)
            {
                DamageToTower(enterDamage);
                hasAttacked = true;
                Debug.Log($"DamageToTower called at time {Time.time}");
            }
            else if (nearPlayer)
            {
                DamageToPlayer(enterDamage);
                hasAttacked = true;
                Debug.Log($"DamageToPlayer called at time {Time.time}");
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
                DamageToTower(); // attack once
            }
            else if (currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange)
            {
                animator.SetTrigger(AttackHash);
                DamageToPlayer(); // attack once
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
            Debug.Log($"Spider dealt {enterDamage} (animation) to Player.");
        }
    }

    protected void DamageToPlayer(float damageAmount)
    {
        if (playerController != null)
        {
            playerController.TakeDamage(damageAmount);
            Debug.Log($"Spider dealt {damageAmount} periodic damage to Player.");
        }
    }

    protected void DamageToTower(float damageAmount)
    {
        if (tower != null)
        {
            tower.TakeDamage(damageAmount);
            Debug.Log($"Spider dealt {damageAmount} damage to Tower.");
        }
    }

    protected override void StayDamageToPlayer()
    {
        if (playerController == null) return;

        stayDamageTimer += Time.deltaTime;
        if (stayDamageTimer >= stayDamageInterval)
        {
            playerController.TakeDamage(stayDamage);
            Debug.Log($"Spider dealt {stayDamage} stay damage.");
            stayDamageTimer = 0f;
        }
    }

    protected void ResetAttack()
    {
        hasAttacked = false;
    }

    private void CreateExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, explosionDestroyDelay);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TakeDamage(explosionDamageToPlayer);
                    Debug.Log($"Spider explosion dealt {explosionDamageToPlayer} damage to Player.");
                }
            }
            else if (hit.CompareTag("Tower"))
            {
                MainTower mt = hit.GetComponent<MainTower>();
                if (mt != null)
                {
                    mt.TakeDamage(explosionDamageToTower);
                    Debug.Log($"Spider explosion dealt {explosionDamageToTower} damage to Tower.");
                }
            }
        }
    }

    protected override void Die()
    {
        CreateExplosion();
        base.Die();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Spider Trigger Enter: {collision.gameObject.name}");
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
