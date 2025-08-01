using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            Player player = GetComponent<Player>();
            player.TakeDamage(20);
        }
        else if (collision.CompareTag("Coin"))
        {
            int randomMoney = Random.Range(10, 31);

            Player player = GetComponent<Player>();
            player.AddMoney(randomMoney);

            Destroy(collision.gameObject);
        }
    }
}