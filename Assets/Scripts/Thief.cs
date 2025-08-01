using UnityEngine;

public class Thief : Enemy
{
    private bool hasAttacked = false;
    private float attackTimer = 0f;

    [SerializeField] private float attackCooldown = 2f;
    private PlayerController playerController;

    protected override void Awake()
    {
        base.Awake();

        enterDamage = 15f;
        moveSpeed = 1.5f;
    }

    protected override void Update()
    {
        if (GamePauseManager.IsPaused) return;

        UpdateChaseTarget();
        base.Update();

        attackTimer += Time.deltaTime;

        if (hasAttacked && attackTimer >= attackCooldown)
        {
            hasAttacked = false;
            attackTimer = 0f;
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
                animator?.SetTrigger(AttackHash);
                DamageToTower();
                hasAttacked = true;
            }
            else if (nearPlayer)
            {
                animator?.SetTrigger(AttackHash);
                DamageToPlayer();
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
        }
    }

    protected override void MoveToPlayer()
    {
        if (GamePauseManager.IsPaused || playerTarget == null) return;

        Vector2 newPosition = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        FlipToTarget(playerTarget);
    }

    protected override void DamageToPlayer()
    {
        if (playerTarget == null || playerController == null) return;

        playerController.TakeDamage(enterDamage);
        Debug.Log($"Thief dealt {enterDamage} damage to Player!");
    }

    protected void DamageToTower()
    {
        if (tower != null)
        {
            tower.TakeDamage(enterDamage);
            Debug.Log($"Thief dealt {enterDamage} damage to Tower!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTarget = collision.transform;
            playerController = playerTarget.GetComponent<PlayerController>();
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
