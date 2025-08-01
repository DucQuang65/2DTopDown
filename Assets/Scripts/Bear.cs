using UnityEngine;

public class Bear : Enemy
{
    private bool hasAttacked = false;
    private float attackTimer = 0f;
    [SerializeField] private float attackCooldown = 1.5f;

    private PlayerController playerController;

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
    }

    private void UpdateChaseTarget()
    {
        if (playerTarget != null)
        {
            if (playerController == null)
                playerController = playerTarget.GetComponent<PlayerController>();

            if (playerController != null && !playerController.IsDead && playerController.currentHp > 0)
            {
                currentState = EnemyState.ChasingPlayer;
                return;
            }
            else
            {
                playerTarget = null; // Player chết hoặc không tồn tại -> bỏ target
            }
        }

        // Nếu không có Player hợp lệ thì kiểm tra Tower
        if (tower != null && tower.GetCurrentHP() > 0)
        {
            currentState = EnemyState.ChasingTower;
            return;
        }

        // Không có target nào
        currentState = EnemyState.Idle;
    }

    protected override void HandleAnimation(float distanceToPlayer)
    {
        if (GamePauseManager.IsPaused) return;

        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;

        if (animator != null)
        {
            animator.SetBool("IsRun", currentState == EnemyState.ChasingPlayer || currentState == EnemyState.ChasingTower);

            if (currentState == EnemyState.ChasingPlayer && distanceToPlayer <= attackRange)
            {
                animator.SetTrigger("Attack");
                TryDamagePlayer();
            }
            else if (currentState == EnemyState.ChasingTower && distanceToTower <= attackRange)
            {
                animator.SetTrigger("Attack");
                TryDamageTower();
            }
            else
            {
                ResetAttack();
            }
        }
    }

    private void TryDamagePlayer()
    {
        if (!hasAttacked && playerTarget != null)
        {
            PlayerController playerComp = playerTarget.GetComponent<PlayerController>();
            if (playerComp != null && !playerComp.IsDead)
            {
                playerComp.TakeDamage(enterDamage);
                hasAttacked = true;
                Debug.Log($"Bear dealt {enterDamage} damage to Player!");
            }
        }
    }

    private void TryDamageTower()
    {
        if (!hasAttacked && tower != null && tower.GetCurrentHP() > 0)
        {
            tower.TakeDamage(enterDamage);
            hasAttacked = true;
            Debug.Log($"Bear dealt {enterDamage} damage to Tower!");
        }
    }

    protected void ResetAttack()
    {
        hasAttacked = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Tower"))
        {
            MainTower towerHit = collision.GetComponent<MainTower>();
            if (towerHit != null)
            {
                towerHit.TakeDamage(stayDamage * Time.deltaTime);
                Debug.Log("Bear damaging Tower continuously!");
            }
        }
        else if (collision.CompareTag("Player"))
        {
            stayDamageTimer += Time.deltaTime;

            if (stayDamageTimer >= stayDamageInterval)
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    player.TakeDamage(stayDamage);
                    Debug.Log($"Bear dealt {stayDamage} damage to Player during stay!");
                }
                stayDamageTimer = 0f;
            }
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
}
