using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject mainTower;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button levelUpButton;

    private MainTower tower;

    private void Start()
    {
        tower = mainTower.GetComponent<MainTower>();
        tower.onTowerClicked.AddListener(OnTowerClicked);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (statsPanel != null)
            statsPanel.SetActive(true);

        if (levelUpButton != null)
            levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);

        // Lắng nghe event thay đổi vàng từ GoldManager
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged += OnGoldChanged;

        UpdateUI();
    }
    private void Update()
    {
        // Nếu bảng upgrade đang hiển thị và có click chuột trái
        if (upgradePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            // Kiểm tra xem click có đang trên UI không (để không tắt khi click UI khác)
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                // Kiểm tra xem click có trúng MainTower hay không
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

                // Dùng raycast 2D để kiểm tra
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider == null || hit.collider.gameObject != mainTower)
                {
                    // Click ngoài MainTower → tắt upgrade panel
                    upgradePanel.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (tower != null)
            tower.onTowerClicked.RemoveListener(OnTowerClicked);

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int newGold)
    {
        UpdateUI();
    }

    private void OnTowerClicked()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            UpdateUI();
        }
    }

    private void OnLevelUpButtonClicked()
    {
        if (tower != null && upgradePanel != null && GoldManager.Instance != null)
        {
            float upgradeCost = tower.GetUpgradeCost();

            if (!tower.IsMaxLevel() && GoldManager.Instance.HasEnoughGold((int)upgradeCost))
            {
                if (tower.Upgrade(upgradeCost))
                {
                    GoldManager.Instance.SpendGold((int)upgradeCost);
                    Debug.Log($"Nâng cấp thành công! Còn {GoldManager.Instance.currentGold} vàng");
                    UpdateUI();

                    AudioSource audioSource = GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(audioSource.clip);
                    }
                }
            }
            else
            {
                Debug.Log("Không đủ vàng hoặc đã đạt cấp tối đa");
            }
        }
    }

    private void UpdateUI()
    {
        if (tower == null) return;

        float upgradeCost = tower.GetUpgradeCost();
        bool isMax = tower.IsMaxLevel();
        int currentGold = GoldManager.Instance != null ? GoldManager.Instance.currentGold : 0;

        if (upgradeCostText != null)
            upgradeCostText.text = isMax ? "Max Level" : $"Upgrade Cost: {upgradeCost}";

        if (hpText != null)
            hpText.text = $"HP: {tower.GetCurrentHP()}/{tower.GetMaxHP()}";

        if (armorText != null)
            armorText.text = $"Armor: {tower.GetArmor()}";

        if (levelText != null)
            levelText.text = $"Level: {tower.GetLevel()}";

        if (levelUpButton != null)
            levelUpButton.interactable = !isMax && currentGold >= upgradeCost;
    }
}
