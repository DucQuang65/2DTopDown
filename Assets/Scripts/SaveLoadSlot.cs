using UnityEngine;
using TMPro;
using static SaveLoadData;
using UnityEngine.SceneManagement;

public class SaveLoadSlot : MonoBehaviour
{
    public int slotNumber; // Số slot: 1,2,3
    public SaveLoadManager saveLoadManager;
    public EnemySpawner enemySpawner;
    public TextMeshProUGUI saveInfoText;

    private string SaveName => $"Save_{slotNumber}";

    private void Start()
    {
        if (saveLoadManager == null) saveLoadManager = FindObjectOfType<SaveLoadManager>();
        if (enemySpawner == null) enemySpawner = FindObjectOfType<EnemySpawner>();

        RefreshSaveInfo();
    }

    public void Save()
    {
        var player = FindObjectOfType<PlayerController>();
        var goldManager = GoldManager.Instance;

        if (player == null || goldManager == null || enemySpawner == null)
        {
            Debug.LogError("Không tìm thấy Player, GoldManager hoặc EnemySpawner.");
            return;
        }

        saveLoadManager.SaveGameData(SaveName, enemySpawner, goldManager, player);
        RefreshSaveInfo();
    }

    public void Load()
    {
        var gameData = saveLoadManager.Load<GameSaveData>(SaveName);
        if (gameData != null)
        {
            GameData.slotToLoad = slotNumber;
            SceneManager.LoadScene("Game");
        }
        else
        {
            saveInfoText.text = $"Slot {slotNumber} chưa có dữ liệu save.";
        }
    }

    public void RefreshSaveInfo()
    {
        var gameData = saveLoadManager.Load<GameSaveData>(SaveName);
        if (gameData != null)
        {
            var data = gameData.enemySpawnerSaveData;
            saveInfoText.text = $"Slot {slotNumber}:\nRound {data.round}, Enemies {data.currentEnemyCount}";
        }
        else
        {
            saveInfoText.text = $"Slot {slotNumber}: (trống)";
        }
    }
}
