using UnityEngine;
using System;

public class SoldierStats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }

    public event Action OnDie;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0f) return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDie?.Invoke();
        Destroy(gameObject, 3f); // Hoặc trigger animation trước khi destroy
    }
}
