using UnityEngine;
using UnityEngine.UI;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Economy Settings")]
    public int currentMoney = 500; // Tiền ban đầu của người chơi

    [Header("UI References")]
    public Text moneyText; // Text hiển thị số tiền

    public static event Action<int> OnMoneyChanged; // Sự kiện khi tiền thay đổi

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện yêu cầu chi tiền từ TowerController
        TowerController.OnRequestMoneySpent += HandleMoneySpentRequest;
        // Đăng ký lắng nghe sự kiện tháp lên cấp để update UI tổng quát (nếu cần)
        TowerController.OnTowerLeveledUp += OnTowerLeveledUpHandler;
    }

    void OnDisable()
    {
        // Hủy đăng ký khi đối tượng bị vô hiệu hóa hoặc hủy bỏ
        TowerController.OnRequestMoneySpent -= HandleMoneySpentRequest;
        TowerController.OnTowerLeveledUp -= OnTowerLeveledUpHandler;
    }

    void Start()
    {
        UpdateMoneyUI();

    }

    // Thêm tiền vào tài khoản
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot add negative money. Use SpendMoney instead.");
            return;
        }
        currentMoney += amount;
        UpdateMoneyUI();
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"Added {amount} money. Current: {currentMoney}");
    }

    // Chi tiền từ tài khoản
    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot spend negative money.");
            return false;
        }

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateMoneyUI();
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"Spent {amount} money. Remaining: {currentMoney}");
            return true;
        }
        else
        {
            Debug.Log("Not enough money! Required: " + amount + ", Have: " + currentMoney);
            return false;
        }
    }

    // Hàm xử lý yêu cầu chi tiền từ TowerController
    // Hàm xử lý yêu cầu chi tiền từ TowerController
    private void HandleMoneySpentRequest(TowerController tower, int amountToSpend)
    {
        if (SpendMoney(amountToSpend))
        {
            tower.UpgradeTowerConfirmed(); // Nếu giao dịch thành công, thông báo cho tháp
        }
        else
        {
            Debug.Log("Upgrade failed: Not enough money.");
            // Bạn có thể thông báo cho TowerController rằng không đủ tiền nếu muốn UI của nó phản hồi
        }
    }
    
    // Hàm được gọi khi một tháp lên cấp (để có thể cập nhật UI tổng quát nếu cần)
    private void OnTowerLeveledUpHandler(TowerController tower)
    {
        Debug.Log($"Global: {tower.towerData.towerName} has reached Level {tower.currentLevel}!");
        // Bạn có thể kích hoạt các hiệu ứng đặc biệt toàn cầu ở đây
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: {currentMoney}";
        }
    }
}