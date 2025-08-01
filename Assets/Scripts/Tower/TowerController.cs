using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerController : MonoBehaviour
{
    [Header("Tower Data")]
    public TowerData towerData;

    [Header("Current Stats")]
    public int currentLevel = 1;
    public int currentXP = 0;

    private float currentDamage;
    private float currentFireRate;
    private float currentRange;

    [Header("UI References (Assign in Inspector)")]
    public Text levelText;
    public Text xpText;
    public Text upgradeCostText;
    public Button upgradeButton;

    [Header("Visual Effects (Assign in Inspector)")]
    public GameObject upgradeVFXPrefab;
    public Transform vfxSpawnPoint;
    public GameObject[] towerVisualModels;

    [Header("Audio Effects (Assign in Inspector)")]
    public AudioSource audioSource;
    public AudioClip upgradeSuccessSFX;
    public AudioClip upgradeFailedSFX;

    public static event Action<TowerController, int> OnRequestMoneySpent;
    public static event Action<TowerController> OnTowerLeveledUp;
    public static event Action<TowerController> OnTowerUIUpdate;

    void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("TowerData is not assigned to " + gameObject.name + ". Disabling script.", this);
            enabled = false;
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 1, towerData.levels.Count);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        ApplyLevel();
        UpdateTowerVisual();
        UpdateTowerUI();
    }

    public void AddXP(int amount)
    {
        if (currentLevel >= towerData.levels.Count)
        {
            Debug.Log($"[{towerData.towerName}] is already at max level ({currentLevel}). Cannot gain more XP.");
            currentXP = 0;
            UpdateTowerUI();
            return;
        }

        currentXP += amount;
        Debug.Log($"[{towerData.towerName}] gained {amount} XP. Current XP: {currentXP}");
        UpdateTowerUI();

        if (currentLevel < towerData.levels.Count)
        {
            TowerLevel nextLevelStats = towerData.levels[currentLevel];
            if (currentXP >= nextLevelStats.requiredXPToNextLevel)
            {
                Debug.Log($"[{towerData.towerName}] has enough XP to upgrade!");
                UpdateTowerUI();
            }
        }
    }

    public void RequestUpgrade()
    {
        if (currentLevel >= towerData.levels.Count)
        {
            Debug.Log($"[{towerData.towerName}] is already at max level ({currentLevel}).");
            PlaySFX(upgradeFailedSFX);
            return;
        }

        TowerLevel nextLevelStats = towerData.levels[currentLevel];

        if (currentXP < nextLevelStats.requiredXPToNextLevel)
        {
            Debug.Log($"[{towerData.towerName}] needs more XP to upgrade. Required: {nextLevelStats.requiredXPToNextLevel}, Current: {currentXP}");
            PlaySFX(upgradeFailedSFX);
            return;
        }

        OnRequestMoneySpent?.Invoke(this, nextLevelStats.upgradeCost);
        if (OnRequestMoneySpent == null)
        {
            Debug.LogWarning("OnRequestMoneySpent event has no listeners. Assuming success for testing.");
            UpgradeTowerConfirmed();
        }
    }

    public void UpgradeTowerConfirmed()
    {
        if (currentLevel >= towerData.levels.Count) return;

        currentLevel++;
        currentXP = 0;
        ApplyLevel();
        UpdateTowerVisual();
        PlayUpgradeVFX();
        PlaySFX(upgradeSuccessSFX);
        StartCoroutine(PunchScale());
        OnTowerLeveledUp?.Invoke(this);
        UpdateTowerUI();
    }

    private void ApplyLevel()
    {
        TowerLevel level = towerData.levels[currentLevel - 1];
        currentDamage = level.damage;
        currentFireRate = level.attackSpeed;
        currentRange = level.range;
    }

    private void UpdateTowerUI()
    {
        if (towerData == null) return;

        if (levelText != null) levelText.text = $"Level: {currentLevel}";
        if (xpText != null) xpText.text = $"XP: {currentXP}";

        if (upgradeButton != null)
        {
            if (currentLevel < towerData.levels.Count)
            {
                TowerLevel nextLevelStats = towerData.levels[currentLevel];
                if (upgradeCostText != null)
                {
                    upgradeCostText.text = $"Cost: ${nextLevelStats.upgradeCost}\nXP: {currentXP}/{nextLevelStats.requiredXPToNextLevel}";
                }

                bool canAffordXP = currentXP >= nextLevelStats.requiredXPToNextLevel;
                bool canAffordMoney = EconomyManager.Instance?.currentMoney >= nextLevelStats.upgradeCost;

                upgradeButton.interactable = canAffordXP && canAffordMoney;
            }
            else
            {
                if (upgradeCostText != null) upgradeCostText.text = "MAX LEVEL";
                upgradeButton.interactable = false;
            }
        }

        OnTowerUIUpdate?.Invoke(this);
    }

    private void UpdateTowerVisual()
    {
        int visualIndex = currentLevel - 1;

        if (towerVisualModels != null && towerVisualModels.Length > 0 && visualIndex < towerVisualModels.Length)
        {
            for (int i = 0; i < towerVisualModels.Length; i++)
            {
                if (towerVisualModels[i] != null)
                    towerVisualModels[i].SetActive(i == visualIndex);
            }
        }
    }

    private void PlayUpgradeVFX()
    {
        if (upgradeVFXPrefab == null) return;

        Vector3 spawnPos = (vfxSpawnPoint != null) ? vfxSpawnPoint.position : transform.position;
        GameObject vfxInstance = Instantiate(upgradeVFXPrefab, spawnPos, Quaternion.identity);

        ParticleSystem ps = vfxInstance.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(vfxInstance, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(vfxInstance, 3f);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public float GetCurrentDamage() => currentDamage;
    public float GetCurrentFireRate() => currentFireRate;
    public float GetCurrentRange() => currentRange;
    public int GetCurrentTowerLevel() => currentLevel;
    public int GetCurrentTowerXP() => currentXP;
    public string GetTowerName() => towerData != null ? towerData.towerName : "Unnamed Tower";

    private IEnumerator PunchScale()
    {
        Vector3 original = transform.localScale;
        Vector3 punch = original * 1.2f;

        float duration = 0.15f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(original, punch, t / duration);
            yield return null;
        }

        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(punch, original, t / duration);
            yield return null;
        }
    }

}
