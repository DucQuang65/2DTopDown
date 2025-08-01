using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 10f;
    private float currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    private Transform target;

    [Header("Damage To Tower")]
    public float damageToTower = 5f;

    private void Start()
    {
        currentHealth = maxHealth;

        //// Tìm MainTower theo tag hoặc tên
        //GameObject towerObj = GameObject.FindWithTag("MainTower");
        //if (towerObj != null)
        //{
        //    target = towerObj.transform;
        //}
        //else
        //{
        //    Debug.LogError("Không tìm thấy MainTower (phải gán tag 'MainTower')");
        //}
    }

    private void Update()
    {
        //if (target != null)
        //{
        //    // Di chuyển về phía MainTower
        //    Vector2 direction = (target.position - transform.position).normalized;
        //    transform.Translate(direction * moveSpeed * Time.deltaTime);
        //}
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} bị trúng đạn! Máu còn lại: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} đã chết");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MainTower"))
        {
            MainTower tower = collision.GetComponent<MainTower>();
            if (tower != null)
            {
                tower.TakeDamage(damageToTower);
            }
            Die(); // Enemy tự hủy sau khi gây sát thương
        }
    }
}
