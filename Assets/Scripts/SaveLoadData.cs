using UnityEngine;

public static class SaveLoadData
{
    [System.Serializable]
    public class EnemySpawnerSaveData
    {
        public int round;
        public float timeBetweenSpawns;
        public int currentEnemyCount;
    }

    [System.Serializable]
    public class GoldManagerSaveData
    {
        public int currentGold;
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public SerializableVector3 position;
        public float currentHp;
        public int level;
        public float exp;
        public float expToNextLevel;
        public float currentAtk;
        public float currentAspd;
        public float currentMaxHp;
    }

    [System.Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [System.Serializable]
    public class EnemySaveData
    {
        public SerializableVector3 position;
        public int enemyTypeIndex; // index trong enemies hoặc BossEnemy
        public float currentHp;
    }

    [System.Serializable]
    public class SubTowerCostData
    {
        public string placerID;
        public float currentCost;
    }

    [System.Serializable]
    public class GameSaveData
    {
        public EnemySpawnerSaveData enemySpawnerSaveData;
        public GoldManagerSaveData goldManagerSaveData;
        public PlayerSaveData playerSaveData;
        public EnemySaveData[] enemyList;
        public SubTowerCostData[] subTowerCosts;
    }
}
