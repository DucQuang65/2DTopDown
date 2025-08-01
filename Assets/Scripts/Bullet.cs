using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction; // Direction of travel.
    private float speed; // Movement speed.
    private float damage; // Damage to apply.


    private void Start()
    {
        Debug.Log("Bullet Start chạy");
    }
    public void Setup(Vector2 dir, float spd, float dmg)
    {
        Debug.Log($"Bullet khởi tạo: hướng={dir}, tốc độ={spd}, sát thương={dmg}");
        direction = dir;
        speed = spd;
        damage = dmg;
        transform.right = direction; // Align transform to direction.
    }

    private void Update()
    {
        Debug.Log($"Bullet at {transform.position}");
        transform.Translate(direction * speed * Time.deltaTime, Space.World); // Move in world space (independent of parent).
        Destroy(gameObject, 5f); // Auto-destroy after 5 seconds to prevent leaks.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bullet va chạm với: {other.gameObject.name}, tag={other.tag}, Collider={other}, position={transform.position}, enemy position={other.transform.position}");
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Bullet trúng enemy tại {other.transform.position} với {damage} sát thương");
            Enemy enemy = other.GetComponent<Enemy>(); // Changed to Enemy (abstract class, works for subclasses like HarpoonFish).
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Call TakeDamage (virtual, so subclasses can override).
            }
            Destroy(gameObject); // Destroy bullet on hit.
        }
    }
}