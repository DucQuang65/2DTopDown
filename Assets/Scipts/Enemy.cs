using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{   // Public variables
    [SerializeField] protected float moveSpeed = 2f;
    protected Player player;
    [SerializeField] protected float maxHp = 50f;
    protected float currentHp;
    [SerializeField] private Image hpBar;
    [SerializeField] protected float enterDamage = 10f;
    [SerializeField] protected float stayDamage = 1f;

    protected virtual void Start()
    {
        player = Object.FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!"); // Warn forgot to assign the reference
        }
        currentHp = maxHp; // Initialize current HP to max HP
        UpdateHpBar();
    }
    protected virtual void Update()
    {
        MoveToPlayer();
    }
    protected void MoveToPlayer()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            FlipEnemy();
        }
    }
    protected void FlipEnemy()
    {
        if (player != null)
        {
            transform.localScale = new Vector3(player.transform.position.x < transform.position.x ? -1 : 1, 1, 1);
        }
    }
    public virtual void TakeDamage(float damage)
    {
        // Handle enemy taking damage
        Debug.Log($"Enemy took {damage} damage!");
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0); // Ensure current HP does not go below 0
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        // Handle enemy death
        Destroy(gameObject); // Destroy the enemy game object
    }
    protected void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = currentHp / maxHp; // Update the HP bar fill amount
        }
    }
}
