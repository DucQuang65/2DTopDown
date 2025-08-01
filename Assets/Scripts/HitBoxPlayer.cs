using UnityEngine;

public class HitBoxPlayer : MonoBehaviour
{
    public float damage;
    public bool isDeal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDeal) return;

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                isDeal = true;
                Debug.Log("✅ Hit Enemy: " + other.name);
            }
        }
    }
}
