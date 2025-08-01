using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float damage = 30f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        if (collision.CompareTag("Tower"))
        {
            MainTower tower = collision.GetComponent<MainTower>();
            if (tower != null)
            {
                tower.TakeDamage(damage);
            }
        }
    }

    public void DestroyExplosion()
    {
        Destroy(gameObject);
    }
}
