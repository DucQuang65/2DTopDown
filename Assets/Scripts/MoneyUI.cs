using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private Text moneyText;

    private Player player;

    void Start()
    {
        player = FindAnyObjectByType<Player>();

        if (moneyText == null)
        {
            moneyText = GetComponent<Text>();
        }

        UpdateMoneyDisplay();
    }

    void Update()
    {
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (player != null && moneyText != null)
        {
            moneyText.text = "Money: " + player.money.ToString();
        }
        else
        {
            Debug.LogWarning("MoneyUI: Player or Text reference is missing!");
        }
    }
}