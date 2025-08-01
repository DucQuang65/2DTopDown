using System.Collections.Generic;
using UnityEngine;
using static SaveLoadData;

public class GameStartManager : MonoBehaviour
{
    public SaveLoadManager saveLoadManager;
    public EnemySpawner enemySpawner;

    private void Awake()
    {
        if (saveLoadManager == null)
            saveLoadManager = FindObjectOfType<SaveLoadManager>();
        if (enemySpawner == null)
            enemySpawner = FindObjectOfType<EnemySpawner>();
    }

    private void Start()
    {
        if (GameData.slotToLoad > 0) // thường slot 0 là chưa chọn save
        {
            string saveName = $"Save_{GameData.slotToLoad}";
            var gameData = saveLoadManager.Load<GameSaveData>(saveName);

            if (gameData != null)
            {
                // Load EnemySpawner cơ bản
                var enemyData = gameData.enemySpawnerSaveData;
                enemySpawner.Round = enemyData.round;
                enemySpawner.TimeBetweenSpawns = enemyData.timeBetweenSpawns;
                enemySpawner.CurrentEnemyCount = enemyData.currentEnemyCount;
                enemySpawner.UpdateRoundUI();
                enemySpawner.UpdateEnemyCountUI();
                enemySpawner.StopAllCoroutines();

                // Load enemy list
                if (gameData.enemyList != null && gameData.enemyList.Length > 0)
                {
                    var savedEnemies = new List<EnemySpawner.SavedEnemyData>();
                    foreach (var e in gameData.enemyList)
                    {
                        savedEnemies.Add(new EnemySpawner.SavedEnemyData()
                        {
                            position = e.position.ToVector3(),
                            enemyTypeIndex = e.enemyTypeIndex,
                            currentHp = e.currentHp,
                            isBoss = (e.enemyTypeIndex >= enemySpawner.enemies.Count)
                        });
                    }
                    enemySpawner.LoadEnemiesState(savedEnemies);
                }
                enemySpawner.StartCoroutine(enemySpawner.SpawnRounds());

                // Load Player
                var player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
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
                }

                // Load Gold
                var goldManager = GoldManager.Instance;
                if (goldManager != null)
                {
                    goldManager.SetGold(gameData.goldManagerSaveData.currentGold);
                }

                // Load lại giá xây SubTower từ save
                var placers = GameObject.FindObjectsOfType<BuildingPlacer>();
                if (gameData.subTowerCosts != null)
                {
                    foreach (var savedCost in gameData.subTowerCosts)
                    {
                        foreach (var placer in placers)
                        {
                            if (placer.placerID == savedCost.placerID)
                            {
                                placer.InitializeCost(savedCost.currentCost);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Không có dữ liệu save hợp lệ.");
            }
        }
        else
        {
            Debug.Log("Không có slot load.");
        }
    }
}
