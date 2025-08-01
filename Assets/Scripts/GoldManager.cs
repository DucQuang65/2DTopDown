using TMPro;
using UnityEngine;
using System;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int currentGold = 0;
    public TextMeshProUGUI goldText;

    // Sự kiện vàng thay đổi
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateGoldText();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
        UpdateGoldText();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            UpdateGoldText();
            return true;
        }
        return false;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    // Phương thức mới để đặt giá trị vàng và cập nhật UI cùng sự kiện
    public void SetGold(int amount)
    {
        currentGold = amount;
        OnGoldChanged?.Invoke(currentGold);
        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
        else
        {
            Debug.LogWarning("Gold Text UI chưa được gán trong GoldManager!");
        }
    }
}
