using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Normal,         
        Following,      
        StaticExplosion
    }

    [SerializeField] private ProjectileType projectileType = ProjectileType.Normal;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    private Vector3 moveDirection;
    private Transform target;// For following type projectile

    private void Start()
    {
        if (projectileType == ProjectileType.StaticExplosion)
        {
            Explode(transform.position);
        }
        else
        {
            Destroy(gameObject, lifeTime);
        }
    }

    private void Update()
    {
        switch (projectileType)
        {
            case ProjectileType.Normal:
                transform.position += moveDirection * Time.deltaTime;
                break;

            case ProjectileType.Following:
                if (target != null)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    transform.position += direction * speed * Time.deltaTime;
                }
                break;
        }
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized * speed;
    }

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Tower"))
        {
            Explode(transform.position);
            Destroy(gameObject);
        }
    }

    private void Explode(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, position, Quaternion.identity);
        }
    }
}
