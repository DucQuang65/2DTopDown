using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxRounds = 3;      // Số round tồn tại tối đa
    private int startRound;        // round bắt đầu (để tính vòng đời)
    private int currentRound;      // round hiện tại (đếm từ khi đặt)

    public event Action OnDie;     // Sự kiện khi hết vòng đời

    public void Init(int startRound)
    {
        this.startRound = startRound;
        currentRound = startRound;
    }

    // Gọi khi bước sang round mới
    public void UpdateRound(int newRound)
    {
        if (newRound <= currentRound) return; // nếu round chưa tăng, không làm gì

        currentRound = newRound;

        int roundsPassed = currentRound - startRound;
        int roundsLeft = maxRounds - roundsPassed;

        if (roundsLeft <= 0)
        {
            Die();
        }
    }

    public float GetLifeRatio()
    {
        int roundsPassed = currentRound - startRound;
        int roundsLeft = maxRounds - roundsPassed;
        return Mathf.Clamp01((float)roundsLeft / maxRounds);
    }

    private void Die()
    {
        OnDie?.Invoke();
        Destroy(gameObject);
    }
}
