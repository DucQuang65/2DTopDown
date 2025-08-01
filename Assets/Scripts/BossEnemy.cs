using UnityEngine;

public class BossEnemy : Enemy
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float speedShootBullet = 20f;
    [SerializeField] private float speedCircularShot = 10f;
    [SerializeField] private float hpValue = 20f;
    [SerializeField] private GameObject minionsPrefab;
    [SerializeField] private float skillCooldown = 5f;
    [SerializeField] private GameObject usbPrefab;

    private float nextSkillTime = 0f;
    private bool isPlayerInRange = false;

    protected override void Awake()
    {
        base.Awake();

        // playerTarget đã được xử lý trong lớp cha (Enemy), không cần gán lại.
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTarget = playerObj.transform;
            else
                Debug.LogError("Player with tag 'Player' not found!");
        }

        enterDamage = 20f;
        stayDamage = 10f;
        moveSpeed = 2.5f;
    }

    protected override void Update()
    {
        base.Update();

        if (Time.time >= nextSkillTime)
        {
            UseSkill();
        }

        if (!isPlayerInRange && tower != null)
        {
            Vector2 direction = (tower.transform.position - transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            Debug.Log("Moving toward Tower!");
        }
    }

    protected override void Die()
    {
        if (usbPrefab != null)
        {
            Instantiate(usbPrefab, transform.position, Quaternion.identity);
        }
        base.Die();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Player playerComp = collision.GetComponent<Player>();
            if (playerComp != null)
            {
                playerComp.TakeDamage(enterDamage);
                Debug.Log($"Dealt {enterDamage} damage to Player on enter!");
            }
        }
        else if (collision.CompareTag("Tower"))
        {
            MainTower towerComp = collision.GetComponent<MainTower>();
            if (towerComp != null)
            {
                towerComp.TakeDamage(enterDamage);
                Debug.Log($"Dealt {enterDamage} damage to Tower on enter!");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerComp = collision.GetComponent<PlayerController>();
            if (playerComp != null)
            {
                playerComp.TakeDamage(stayDamage * Time.deltaTime);
                Debug.Log($"Dealing {stayDamage} damage/sec to Player on stay!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left range, resuming Tower target!");
        }
    }

    private void Teleport()
    {
        if (playerTarget != null)
        {
            transform.position = playerTarget.position;
        }
    }

    private void SpawnMinions()
    {
        if (minionsPrefab != null)
        {
            Instantiate(minionsPrefab, transform.position, Quaternion.identity);
            Debug.Log("Spawning minions!");
        }
    }

    private void ShootBullet()
    {
        if (playerTarget != null && bulletPrefab != null && firePoint != null)
        {
            Vector3 directionToPlayer = playerTarget.position - firePoint.position;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(directionToPlayer.normalized * speedShootBullet);
            Debug.Log("Shooting bullet!");
        }
    }

    private void CircularShot()
    {
        const int bulletCount = 12;
        float angleStep = 360f / bulletCount;
        if (bulletPrefab == null || firePoint == null) return;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 bulletDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(bulletDirection * speedCircularShot);
        }
        Debug.Log("Performing circular shot!");
    }

    private void Healing(float hpAmount)
    {
        currentHp = Mathf.Min(currentHp + hpAmount, maxHp);
        UpdateHpBar();
        Debug.Log("Healing boss!");
    }

    private void RandomSkill()
    {
        int randomSkill = Random.Range(0, 5);
        switch (randomSkill)
        {
            case 0: Teleport(); break;
            case 1: ShootBullet(); break;
            case 2: CircularShot(); break;
            case 3: Healing(hpValue); break;
            case 4: SpawnMinions(); break;
        }
    }

    private void UseSkill()
    {
        nextSkillTime = Time.time + skillCooldown;
        RandomSkill();
    }
}
