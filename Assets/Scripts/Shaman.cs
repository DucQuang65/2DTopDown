using UnityEngine;

public class Shaman : Enemy
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject explosionPrefab;

    [SerializeField] private float teleportCooldown = 60f;
    [SerializeField] private float explosionShootCooldown = 20f;
    [SerializeField] private float followingShootCooldown = 10f;
    [SerializeField] private float normalShootCooldown = 5f;

    private float teleportTimer = 0f;
    private float explosionShootTimer = 0f;
    private float followingShootTimer = 0f;
    private float normalShootTimer = 0f;

    // Thêm cooldown giữa các lần gây sát thương
    private bool hasAttacked = false;
    private float damageInterval = 1f;  // Có thể chỉnh phù hợp
    private float damageTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        enterDamage = 20f;
        moveSpeed = 1f;
    }

    protected override void Update()
    {
        if (GamePauseManager.IsPaused) return;
        base.Update();

        teleportTimer += Time.deltaTime;
        explosionShootTimer += Time.deltaTime;
        followingShootTimer += Time.deltaTime;
        normalShootTimer += Time.deltaTime;

        damageTimer += Time.deltaTime;

        if (playerTarget == null && tower == null) return;

        float distanceToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;
        float distanceToTower = tower != null ? Vector2.Distance(transform.position, tower.transform.position) : float.MaxValue;

        bool isPlayerInRange = distanceToPlayer <= 20f;
        bool isTowerInRange = distanceToTower <= 20f;

        animator?.SetBool(IsRunHash, currentState == EnemyState.ChasingPlayer || currentState == EnemyState.ChasingTower);

        // Các hành động đặc biệt vẫn giữ nguyên
        if (isPlayerInRange)
        {
            if (normalShootTimer >= normalShootCooldown)
            {
                NormalShoot();
                normalShootTimer = 0f;
            }

            if (explosionShootTimer >= explosionShootCooldown)
            {
                ExplosionShoot();
                explosionShootTimer = 0f;
            }

            if (followingShootTimer >= followingShootCooldown)
            {
                FollowingBullet();
                followingShootTimer = 0f;
            }

            if (teleportTimer >= teleportCooldown)
            {
                Teleport();
                teleportTimer = 0f;
            }
        }

        // --- Phần attack player và tower theo kiểu Snake ---
        bool isStandingStill = IsStandingStill();

        if (!hasAttacked && isStandingStill)
        {
            // Ưu tiên tấn công mục tiêu gần hơn trong tầm tấn công
            if (isPlayerInRange && distanceToPlayer <= attackRange)
            {
                DamageToPlayer();
                hasAttacked = true;
                damageTimer = 0f;
            }
            else if (isTowerInRange && distanceToTower <= attackRange)
            {
                DamageToTower();
                hasAttacked = true;
                damageTimer = 0f;
            }
        }

        if (hasAttacked && damageTimer >= damageInterval)
        {
            hasAttacked = false;
        }
    }

    private bool IsStandingStill()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        return rb != null ? rb.linearVelocity.sqrMagnitude < 0.01f : true;
    }

    protected override void MoveToPlayer()
    {
        if (GamePauseManager.IsPaused) return;
        if (playerTarget == null) return;

        Vector2 newPosition = Vector2.MoveTowards(transform.position, playerTarget.position, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        FlipToTarget(playerTarget);
    }

    private void ExplosionShoot()
    {
        if (GamePauseManager.IsPaused) return;
        if (explosionPrefab == null) return;

        Vector3 explosionPos = tower != null ? tower.transform.position : transform.position;

        if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= 20f)
        {
            explosionPos = playerTarget.position;
        }

        Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
        animator?.SetTrigger(AttackHash);
        Debug.Log("Shaman exploded at " + (explosionPos == playerTarget.position ? "Player" : "Tower") + " position!");
    }

    private void FollowingBullet()
    {
        if (GamePauseManager.IsPaused) return;
        if (playerTarget != null && firePoint != null && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                projectile.SetTarget(playerTarget);
            }

            animator?.SetTrigger(AttackHash);
            Debug.Log("Shaman shot following bullet!");
        }
    }

    private void Teleport()
    {
        if (GamePauseManager.IsPaused) return;
        if (playerTarget != null)
        {
            transform.position = playerTarget.position;
            animator?.SetTrigger(AttackHash);
            Debug.Log("Shaman teleported to player!");
        }
    }

    private void NormalShoot()
    {
        if (GamePauseManager.IsPaused) return;
        if (playerTarget != null && firePoint != null && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();
            if (projectile != null)
            {
                Vector3 direction = (playerTarget.position - firePoint.position).normalized;
                projectile.SetDirection(direction);
            }

            animator?.SetTrigger(AttackHash);
            Debug.Log("Shaman shot normal bullet!");
        }
    }

    protected override void DamageToPlayer()
    {
        if (playerTarget == null) return;

        PlayerController player = playerTarget.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(enterDamage);
            Debug.Log($"Shaman dealt {enterDamage} damage to Player.");
        }
    }

    protected override void DamageToTower()
    {
        if (tower == null) return;

        tower.TakeDamage(enterDamage);
        Debug.Log($"Shaman dealt {enterDamage} damage to Tower.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(enterDamage);
                Debug.Log($"Dealt {enterDamage} damage to Player on enter!");
            }
        }
        else if (collision.CompareTag("Tower"))
        {
            MainTower towerHit = collision.GetComponent<MainTower>();
            if (towerHit != null)
            {
                towerHit.TakeDamage(enterDamage);
                Debug.Log($"Dealt {enterDamage} damage to Tower on enter!");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Tower"))
        {
            MainTower towerHit = collision.GetComponent<MainTower>();
            if (towerHit != null)
            {
                towerHit.TakeDamage(stayDamage * Time.deltaTime);
                Debug.Log("Shaman damaging Tower continuously!");
            }
        }
        else if (collision.CompareTag("Player"))
        {
            stayDamageTimer += Time.deltaTime;

            if (stayDamageTimer >= stayDamageInterval)
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(stayDamage);
                    Debug.Log($"Shaman dealt {stayDamage} damage to Player during stay!");
                }

                stayDamageTimer = 0f;
            }
        }
    }
}
