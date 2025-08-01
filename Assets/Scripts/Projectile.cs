using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 5f; 
    [SerializeField] private float damage = 10f;
    private Vector2 direction;

    public void SetDirectionAndDamage(Vector2 dir, float dmg)// Method to set the direction and damage of the projectile
    {
        direction = dir.normalized;
        damage = dmg;
    }

    void Update()
    {
        // Move the projectile each frame in the set direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("MainTower") || other.CompareTag("Tower"))
        {
            MainTower mainTower = other.GetComponent<MainTower>();
            if (mainTower != null)
            {
                mainTower.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}