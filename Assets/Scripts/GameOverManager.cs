using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject defeatUI;          // Giao diện thông báo Defeat
    [SerializeField] private float returnToMenuDelay = 5f; // Thời gian chờ trước khi trở lại Main Menu
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên scene Main Menu

    private bool gameEnded = false;

    private void Start()
    {
        if (defeatUI != null)
            defeatUI.SetActive(false); // Ẩn giao diện ban đầu

        // Đăng ký sự kiện phá hủy từ MainTower (nếu cần)
        MainTower tower = FindObjectOfType<MainTower>();
        if (tower != null)
        {
            tower.onDestroyed.AddListener(TriggerDefeat);
        }
    }

    public void TriggerDefeat()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (defeatUI != null)
            defeatUI.SetActive(true); // Hiện giao diện Defeat

        Debug.Log("Game Over: Defeat!");

        // Trở lại Main Menu sau delay
        Invoke(nameof(BackToMainMenu), returnToMenuDelay);
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
