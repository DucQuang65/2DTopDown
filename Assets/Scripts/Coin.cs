using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1; // Giá trị của coin

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Cộng vàng vào UI khi người chơi nhặt
            GoldManager.Instance.AddGold(value);

            // Hủy coin khỏi scene
            Destroy(gameObject);
        }
    }
}
