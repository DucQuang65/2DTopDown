using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public float baseWeight = 1f;
    public float weightPerRound = 0.1f;
    public bool isRare = false;
    public int minSpawnRound = 1;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<Health> subTowers = new List<Health>();
    [SerializeField] public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
    [SerializeField] public GameObject[] BossEnemy;
    [SerializeField] private Transform[] spawnPoint;

    [SerializeField] private float initialTimeBetweenSpawns = 2f;
    [SerializeField] private float minTimeBetweenSpawns = 0.5f;
    [SerializeField] private float spawnTimeReduction = 0.1f;

    [SerializeField] private float hpIncreasePercent = 20f;
    [SerializeField] private float damageIncreasePercent = 10f;

    [SerializeField] private MainTower mainTower;

    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI timeSpawns;

    [SerializeField] private int maxEnemiesOnScene = 20;

    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private float timeBetweenRounds = 5f;
    [SerializeField] private int round = 1;
    [SerializeField] private int currentEnemyCount = 0;
    private int enemiesToSpawn = 0;
    private int enemiesSpawnedThisRound = 0;

    private bool roundInProgress = false;

    [System.Serializable]
    public class SavedEnemyData
    {
        public int enemyTypeIndex;
        public Vector3 position;
        public float currentHp;
        public bool isBoss;
    }
    [SerializeField]
    private List<Enemy> currentEnemies = new List<Enemy>();

    public float TimeBetweenSpawns
    {
        get => timeBetweenSpawns;
        set => timeBetweenSpawns = value;
    }

    public int Round
    {
        get => round;
        set => round = value;
    }

    public int CurrentEnemyCount
    {
        get => currentEnemyCount;
        set => currentEnemyCount = value;
    }

    void Start()
    {
        timeBetweenSpawns = initialTimeBetweenSpawns;
        UpdateRoundUI();
        UpdateEnemyCountUI();
        StartCoroutine(FirstRoundStart());
    }

    public IEnumerator SpawnRounds()
    {
        while (true)
        {
            roundInProgress = true;
            enemiesToSpawn = CalculateEnemyCountForRound(round);
            enemiesSpawnedThisRound = 0;
            Debug.Log($"[Round {round}] Starting round, will spawn {enemiesToSpawn} enemies.");

            float spawnTimer = 0f;

            while (enemiesSpawnedThisRound < enemiesToSpawn)
            {
                if (GamePauseManager.IsPaused)
                {
                    yield return null;
                    continue;
                }

                spawnTimer += Time.deltaTime;

                if (spawnTimer >= timeBetweenSpawns && currentEnemyCount < maxEnemiesOnScene)
                {
                    SpawnEnemy();
                    spawnTimer = 0f;
                }
                yield return null;
            }

            Debug.Log($"[Round {round}] All enemies spawned. Waiting for player to kill all...");

            while (currentEnemyCount > 0)
            {
                yield return null;
            }
            if (timeSpawns != null)
                timeSpawns.text = "Waiting for next round...";

            Debug.Log($"[Round {round}] Round complete!");

            roundInProgress = false;
            yield return StartCoroutine(CountdownToNextRound(timeBetweenRounds));
            round++;
            timeBetweenSpawns = Mathf.Max(minTimeBetweenSpawns, timeBetweenSpawns - spawnTimeReduction);
            UpdateRoundUI();

            UpdateSubTowersRound(round);

            timeBetweenSpawns = Mathf.Max(minTimeBetweenSpawns, timeBetweenSpawns - spawnTimeReduction);

        }
    }

    private IEnumerator CountdownToNextRound(float delay)
    {
        float countdown = delay;
        timeSpawns.gameObject.SetActive(true);

        while (countdown > 0)
        {
            timeSpawns.text = $"Next Round In: {Mathf.CeilToInt(countdown)}s";
            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }

        timeSpawns.gameObject.SetActive(false);
    }

    private IEnumerator FirstRoundStart()
    {
        float countdown = timeBetweenRounds;
        if (timeSpawns != null)
        {
            timeSpawns.gameObject.SetActive(true);
            while (countdown > 0)
            {
                timeSpawns.text = $"Start in: {Mathf.FloorToInt(countdown)}s";
                yield return new WaitForSeconds(1f);
                countdown -= 1f;
            }
            timeSpawns.gameObject.SetActive(false);
        }

        StartCoroutine(SpawnRounds());
    }

    private int CalculateEnemyCountForRound(int currentRound)
    {
        int baseCount = 10;
        float increasePercent = 0.20f;
        return Mathf.RoundToInt(baseCount * Mathf.Pow(1 + increasePercent, currentRound - 1));
    }

    private void SpawnEnemy()
    {
        if (enemiesSpawnedThisRound >= enemiesToSpawn)
        {
            Debug.LogWarning("Attempted to spawn more enemies than allowed this round.");
            return;
        }

        if (spawnPoint.Length == 0 || (enemies.Count == 0 && BossEnemy.Length == 0))
        {
            Debug.LogError("No spawn points or enemy prefabs assigned.");
            return;
        }

        Transform spawnPos = spawnPoint[Random.Range(0, spawnPoint.Length)];
        bool isBossRound = (round % 5 == 0) && BossEnemy.Length > 0;

        int enemyTypeIndex;

        if (isBossRound)
        {
            enemyTypeIndex = Random.Range(0, BossEnemy.Length);
        }
        else
        {
            enemyTypeIndex = GetWeightedEnemyIndex();
        }

        SpawnEnemyByType(enemyTypeIndex, spawnPos.position, isBossRound);

        currentEnemyCount++;
        enemiesSpawnedThisRound++;
        UpdateEnemyCountUI();
    }

    private int GetWeightedEnemyIndex()
    {
        List<float> weights = new List<float>();
        float totalWeight = 0f;

        for (int i = 0; i < enemies.Count; i++)
        {
            var data = enemies[i];
            if (round < data.minSpawnRound)
            {
                weights.Add(0f);
                continue;
            }
            float weight = data.baseWeight + (round - 1) * data.weightPerRound;

            if (data.isRare)
            {
                weight *= 0.3f;
            }

            weights.Add(weight);
            totalWeight += weight;
        }
        if (totalWeight == 0f)
        {
            Debug.LogWarning("No eligible enemies to spawn this round.");
            return 0;
        }

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }

    public void SpawnEnemyByType(int enemyTypeIndex, Vector3 position, bool isBossRound, float hpToSet = -1f)
    {
        GameObject prefab;
        if (isBossRound)
        {
            if (enemyTypeIndex < 0 || enemyTypeIndex >= BossEnemy.Length)
            {
                Debug.LogError($"Invalid boss enemyTypeIndex: {enemyTypeIndex}");
                return;
            }
            prefab = BossEnemy[enemyTypeIndex];
        }
        else
        {
            if (enemyTypeIndex < 0 || enemyTypeIndex >= enemies.Count)
            {
                Debug.LogError($"Invalid enemyTypeIndex: {enemyTypeIndex}");
                return;
            }
            prefab = enemies[enemyTypeIndex].enemyPrefab;
        }

        GameObject spawnedEnemy = Instantiate(prefab, position, Quaternion.identity);
        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError("Spawned enemy missing Enemy script!");
            Destroy(spawnedEnemy);
            return;
        }

        float hpMultiplier = 1 + (hpIncreasePercent / 100f) * (round - 1);
        enemyScript.maxHp *= hpMultiplier;

        if (hpToSet > 0)
        {
            enemyScript.currentHp = hpToSet;
        }
        else
        {
            enemyScript.currentHp = enemyScript.maxHp;
        }

        float damageMultiplier = 1 + (damageIncreasePercent / 100f) * (round - 1);
        enemyScript.enterDamage *= damageMultiplier;

        enemyScript.tower = mainTower;
        enemyScript.OnEnemyDestroyed += OnEnemyDestroyed;

        enemyScript.enemyTypeIndex = enemyTypeIndex;
        enemyScript.isBoss = isBossRound;

        currentEnemies.Add(enemyScript);
    }

    private void OnEnemyDestroyed(Enemy enemy)
    {
        currentEnemyCount--;
        enemy.OnEnemyDestroyed -= OnEnemyDestroyed;
        currentEnemies.Remove(enemy);
        UpdateEnemyCountUI();
        Debug.Log($"Enemy destroyed. Remaining: {currentEnemyCount}");
    }

    public void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
            enemyCountText.text = $"Enemies: {currentEnemyCount}";
    }

    public void UpdateRoundUI()
    {
        if (roundText != null)
            roundText.text = $"Round: {round}";
    }

    public List<SavedEnemyData> SaveEnemiesState()
    {
        List<SavedEnemyData> savedData = new List<SavedEnemyData>();
        foreach (Enemy enemy in currentEnemies)
        {
            if (enemy != null)
            {
                SavedEnemyData data = new SavedEnemyData()
                {
                    enemyTypeIndex = enemy.enemyTypeIndex,
                    position = enemy.transform.position,
                    currentHp = enemy.currentHp,
                    isBoss = enemy.isBoss,
                };
                savedData.Add(data);
            }
        }
        return savedData;
    }

    public void LoadEnemiesState(List<SavedEnemyData> savedEnemies)
    {
        foreach (Enemy e in currentEnemies)
        {
            if (e != null) Destroy(e.gameObject);
        }
        currentEnemies.Clear();

        currentEnemyCount = 0;

        foreach (var data in savedEnemies)
        {
            SpawnEnemyByType(data.enemyTypeIndex, data.position, data.isBoss, data.currentHp);
            currentEnemyCount++;
        }

        UpdateEnemyCountUI();
    }
    // Gọi khi đặt SubTower mới:
    public void RegisterSubTower(Health subTowerHealth)
    {
        if (!subTowers.Contains(subTowerHealth))
            subTowers.Add(subTowerHealth);
    }

    // Gọi khi xóa SubTower (nếu có)
    public void UnregisterSubTower(Health subTowerHealth)
    {
        if (subTowers.Contains(subTowerHealth))
            subTowers.Remove(subTowerHealth);
    }

    private void UpdateSubTowersRound(int newRound)
    {
        for (int i = subTowers.Count - 1; i >= 0; i--)
        {
            if (subTowers[i] != null)
            {
                subTowers[i].UpdateRound(newRound);
            }
            else
            {
                // SubTower đã bị destroy
                subTowers.RemoveAt(i);
            }
        }
    }
    public void PlaceSubTower(GameObject subTowerPrefab, Vector3 position)
    {
        GameObject newSubTower = Instantiate(subTowerPrefab, position, Quaternion.identity);

        Health health = newSubTower.GetComponent<Health>();
        if (health != null)
        {
            health.Init(round);  // Gán vòng hiện tại
            RegisterSubTower(health); // Quản lý subtower
        }
    }
}
