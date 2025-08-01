using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static SaveLoadData;

public class SaveLoadController : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public PlayerController player;
    public GoldManager goldManager;
    public SaveLoadManager saveLoadManager;
    public TextMeshProUGUI saveInfoText;
    public List<BuildingPlacer> buildingPlacers;

    private void Awake()
    {
        if (!enemySpawner) enemySpawner = FindObjectOfType<EnemySpawner>();
        if (!player) player = FindObjectOfType<PlayerController>();
        if (!goldManager) goldManager = GoldManager.Instance;
        if (!saveLoadManager) saveLoadManager = FindObjectOfType<SaveLoadManager>();
    }

    public void SaveGame()
    {
        if (enemySpawner == null || player == null || goldManager == null || saveLoadManager == null)
        {
            Debug.LogWarning("Thiếu thành phần để lưu game!");
            saveInfoText.text = "Lưu thất bại: thiếu thành phần.";
            return;
        }

        string saveName = "Save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Tạo data từng phần
        var enemyData = new EnemySpawnerSaveData()
        {
            round = enemySpawner.Round,
            timeBetweenSpawns = enemySpawner.TimeBetweenSpawns,
            currentEnemyCount = enemySpawner.CurrentEnemyCount
        };

        var goldData = new GoldManagerSaveData()
        {
            currentGold = goldManager.currentGold
        };

        var playerData = new PlayerSaveData()
        {
            position = new SerializableVector3(player.transform.position),
            currentHp = player.currentHp,
            level = player.level,
            exp = player.exp,
            expToNextLevel = player.expToNextLevel,
            currentAtk = player.currentAtk,
            currentAspd = player.currentAspd,
            currentMaxHp = player.currentMaxHp
        };

        var placers = FindObjectsOfType<BuildingPlacer>();
        var costs = new List<SubTowerCostData>();

        foreach (var placer in placers)
        {
            costs.Add(new SubTowerCostData()
            {
                placerID = placer.placerID,
                currentCost = placer.currentCost // hoặc placer.currentCost nếu biến là public
            });
        }

        var gameData = new GameSaveData()
        {
            enemySpawnerSaveData = enemyData,
            goldManagerSaveData = goldData,
            playerSaveData = playerData,
            subTowerCosts = costs.ToArray()
        };



        saveLoadManager.Save(gameData, saveName);

        saveInfoText.text = $"<b>Đã lưu:</b> {saveName}\n" +
            $"Round: <b>{enemyData.round}</b>, Enemies: <b>{enemyData.currentEnemyCount}</b>\n" +
            $"Gold: <b>{goldData.currentGold}</b>, Player Level: <b>{playerData.level}</b>";
    }

    public void LoadLatestGame()
    {
        string[] saveFiles = saveLoadManager.GetAllSaveFiles();
        if (saveFiles.Length == 0)
        {
            saveInfoText.text = "Không có bản save nào.";
            return;
        }

        // Lấy file save mới nhất (theo thời gian tạo, giả sử tên file đúng format)
        string latestFile = saveFiles[saveFiles.Length - 1];
        string saveName = Path.GetFileNameWithoutExtension(latestFile);

        var gameData = saveLoadManager.Load<GameSaveData>(saveName);
        if (gameData != null)
        {
            // Load EnemySpawner
            var enemyData = gameData.enemySpawnerSaveData;
            enemySpawner.Round = enemyData.round;
            enemySpawner.TimeBetweenSpawns = enemyData.timeBetweenSpawns;
            enemySpawner.CurrentEnemyCount = enemyData.currentEnemyCount;
            enemySpawner.UpdateRoundUI();
            enemySpawner.UpdateEnemyCountUI();
            enemySpawner.StopAllCoroutines();
            enemySpawner.StartCoroutine(enemySpawner.SpawnRounds());

            // Load Player
            var pData = gameData.playerSaveData;
            player.transform.position = pData.position.ToVector3();
            player.currentHp = pData.currentHp;
            player.level = pData.level;
            player.exp = pData.exp;
            player.expToNextLevel = pData.expToNextLevel;
            player.currentAtk = pData.currentAtk;
            player.currentAspd = pData.currentAspd;
            player.currentMaxHp = pData.currentMaxHp;
            player.UpdateHpBar();
            player.UpdateAllStatText();

            // Load Gold
            goldManager.SetGold(gameData.goldManagerSaveData.currentGold);

            saveInfoText.text = $"Đã load: {saveName}\n" +
                $"Round: {enemyData.round}, Enemies: {enemyData.currentEnemyCount}\n" +
                $"Gold: {goldManager.currentGold}, Player Level: {player.level}";

            var placers = FindObjectsOfType<BuildingPlacer>();
            foreach (var placer in placers)
            {
                var data = System.Array.Find(gameData.subTowerCosts, x => x.placerID == placer.placerID);
                if (data != null)
                {
                    placer.SetCurrentCost(data.currentCost);
                }
            }
        }
        else
        {
            saveInfoText.text = "Dữ liệu save không hợp lệ hoặc bị lỗi.";
        }
    }

    public void ShowAllSaves()
    {
        string[] saveFiles = saveLoadManager.GetAllSaveFiles();
        if (saveFiles.Length == 0)
        {
            saveInfoText.text = "Không có bản save nào.";
            return;
        }

        string display = "";
        foreach (string path in saveFiles)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            var gameData = saveLoadManager.Load<GameSaveData>(name);
            if (gameData != null)
            {
                var enemyData = gameData.enemySpawnerSaveData;
                var goldData = gameData.goldManagerSaveData;
                var playerData = gameData.playerSaveData;
                display += $"[Save: {name}]\n";
                display += $"Round: {enemyData.round}, Enemies: {enemyData.currentEnemyCount}, Time: {enemyData.timeBetweenSpawns:F2}s\n";
                display += $"Gold: {goldData.currentGold}, Player Level: {playerData.level}\n\n";
            }
            else
            {
                display += $"[Save: {name}] - Không hợp lệ hoặc lỗi\n\n";
            }
        }

        saveInfoText.text = display;
    }
}
