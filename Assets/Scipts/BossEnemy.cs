using Unity.VisualScripting;
using UnityEngine;

public class BossEnemy : Enemy
{
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float speedShootBullet = 20f;
    [SerializeField] private float speedCircularShot = 10f;
    [SerializeField] private float hpValue = 100f;
    [SerializeField] private GameObject Mions;
    protected override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SpawnMions();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(enterDamage);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(stayDamage);
            }
        }
    }
    private void Teleport()
    {

    }
    private void SpawnMions()
    {
        Instantiate(Mions, transform.position, Quaternion.identity);
        Debug.Log("Spawning minions!");
    }
    private void ShootBullet()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.transform.position - firePoint.position;
            GameObject bullet = Instantiate(bulletPrefabs, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(directionToPlayer * speedShootBullet);
        }
        Debug.Log("Shooting bullet!");
    }
    private void CircularShot()
    {
        const int bulletCount = 12;
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad; // Convert angle to radians
            Vector3 bulletDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            GameObject bullet = Instantiate(bulletPrefabs, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(bulletDirection * speedCircularShot);
        }
        Debug.Log("Performing circular shot!");
    }
    private void Healing(float hpAmount)
    {
        currentHp = Mathf.Min(currentHp + hpAmount, maxHp); // Heal the boss by 20, but not exceeding max HP
        UpdateHpBar();
        Debug.Log("Healing boss!");
    }
}
