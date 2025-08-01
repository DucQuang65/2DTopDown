using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private MainTower mainTower;   // Kéo MainTower vào đây
    [SerializeField] private Image fillImage;       // Kéo Fill (màu đỏ) vào đây
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private float smoothSpeed = 5f;

    private float targetFill = 1f;
    private float currentHP;
    private float maxHP;
    private void Start()
    {
        if (mainTower != null)
        {
            mainTower.onHealthChanged.AddListener(UpdateHealthTarget);
            UpdateHealthTarget();
        }
        else
        {
            Debug.LogError("MainTower chưa được gán vào HealthBarUI!");
        }
    }

    private void Update()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
        }
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
        }
    }

    private void UpdateHealthTarget()
    {
        currentHP = mainTower.GetCurrentHP();
        maxHP = mainTower.GetMaxHP();
        targetFill = currentHP / maxHP;
    }
}
