using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static SaveLoadData;

public class SaveLoadManager : MonoBehaviour
{
    private string saveFolderPath;

    private void Awake()
    {
        string gameFolder = Application.dataPath;
        saveFolderPath = Path.Combine(gameFolder, "Save");

        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);
    }


    public void Save<T>(T data, string saveName)
    {
        string path = Path.Combine(saveFolderPath, saveName + ".dat");
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, data);
        }
        Debug.Log("Saved: " + path);
    }

    public T Load<T>(string saveName)
    {
        string path = Path.Combine(saveFolderPath, saveName + ".dat");
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                return (T)bf.Deserialize(file);
            }
        }
        return default;
    }

    public string[] GetAllSaveFiles()
    {
        return Directory.GetFiles(saveFolderPath, "*.dat");
    }

    public void SaveGameData(string saveName, EnemySpawner enemySpawner, GoldManager goldManager, PlayerController player)
    {
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

        var gameData = new GameSaveData()
        {
            enemySpawnerSaveData = enemyData,
            goldManagerSaveData = goldData,
            playerSaveData = playerData
        };

    var placers = GameObject.FindObjectsOfType<BuildingPlacer>();
        var costs = new List<SubTowerCostData>();
        foreach (var placer in placers)
        {
            costs.Add(new SubTowerCostData()
            {
                placerID = placer.placerID,
                currentCost = placer.currentCost
            });
        }
        gameData.subTowerCosts = costs.ToArray();

        var enemyStates = enemySpawner.SaveEnemiesState();

        gameData.enemyList = new EnemySaveData[enemyStates.Count];
        for (int i = 0; i < enemyStates.Count; i++)
        {
            gameData.enemyList[i] = new EnemySaveData()
            {
                position = new SerializableVector3(enemyStates[i].position),
                enemyTypeIndex = enemyStates[i].enemyTypeIndex,
                currentHp = enemyStates[i].currentHp
            };
        }
        Save(gameData, saveName);
    }
    }
