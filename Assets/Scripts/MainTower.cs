using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MainTower : MonoBehaviour
{
    [SerializeField] private float baseMaxHP = 1000f; // Base maximum health points for the tower at level 1.
    [SerializeField] private float currentHP; // Current health points, updated when taking damage or upgrading.
    [SerializeField] private float armor = 10f; // Armor value that reduces incoming damage.
    [SerializeField] private int level = 1; // Current level of the tower, starts at 1.
    [SerializeField] private float hpPerLevel = 200f; // Health increase per level upgrade.
    [SerializeField] private float armorPerLevel = 5f; // Armor increase per level upgrade.
    [SerializeField] private float upgradeCostBase = 100f; // Base cost for upgrades, multiplied by level squared.
    [SerializeField] private float armorPerPoint = 0.04f; // Damage reduction multiplier per armor point.
    [SerializeField] private float maxDamageReduction = 0.8f; // Maximum possible damage reduction (80%).
    [SerializeField] private float maxHP; // Calculated maximum health based on level.
    [SerializeField] private GameObject archerPrefab; // Prefab for spawning archers (sub-towers).
    [SerializeField] private Transform[] archerSpawnPoints = new Transform[3]; // Positions where archers spawn; array size 3 for max archers. Assign in Inspector to fix UnassignedReferenceException.
    private GameObject[] archers = new GameObject[3]; // Array to track spawned archers.
    private const int maxLevel = 10; // Maximum level the tower can reach.

    private float attackDamage = 1f; // Unused in current implementation; could be for future tower attacks.
    private bool isDestroyed = false; // Flag to prevent multiple destruction events.
    private AudioSource audioSource; // For playing sounds, though not used yet.

    // UnityEvents: These allow easy hooking of events in the Inspector without code. Used for UI updates or game logic triggers.
    public UnityEvent onHealthChanged; // Invoked when health changes (e.g., for UI HP bar).
    public UnityEvent onLevelUp; // Invoked on upgrade.
    public UnityEvent onDestroyed; // Invoked when tower is destroyed (game over).
    public UnityEvent<int> onSubTowerUnlocked; // Invoked with count of sub-towers unlocked.
    public UnityEvent onTowerClicked; // Invoked when tower is clicked.

    private void Start()
    {
        maxHP = baseMaxHP; // Initialize max HP to base value.
        currentHP = maxHP; // Set current HP to full.
        audioSource = GetComponent<AudioSource>(); // Get AudioSource component; UnityEngine usage for component retrieval.

        if (archerPrefab == null)
        {
            Debug.LogError("Archer Prefab chưa gán!"); // Log error if prefab not assigned.
        }

        // Check if spawn points are assigned to prevent null reference bugs. Tuition: This loop iterates array (C# foreach possible, but indexed for logs).
        for (int i = 0; i < archerSpawnPoints.Length; i++)
        {
            if (archerSpawnPoints[i] == null)
            {
                Debug.LogError($"Archer spawn point {i} chưa được assigned in Inspector!"); // Warn for each null to guide fix.
            }
        }

        Debug.Log($"MainTower: HP={currentHP}/{maxHP}, Armor={armor}, Level={level}, Damage={attackDamage}");
        onHealthChanged?.Invoke(); // Safe invoke (null-conditional); fixed potential null if no listeners.
        UpdateArchers(); // Spawn initial archers based on level.
    }

    private void OnMouseDown()
    {
        Debug.Log("Clicked MainTower");
        onTowerClicked?.Invoke(); // Handle click event, e.g., show upgrade UI.
    }

    public void TakeDamage(float damage, float pierceFactor = 0f)
    {
        // Calculate effective armor after pierce (clamped between 0 and 1).
        float effectiveArmor = armor * (1f - Mathf.Clamp(pierceFactor, 0f, 1f)); // Mathf.Clamp from UnityEngine for value bounding.
        float damageReduction = effectiveArmor * armorPerPoint; // Reduce damage based on armor.
        damageReduction = Mathf.Clamp(damageReduction, 0f, maxDamageReduction); // Cap reduction.
        float actualDamage = damage * (1f - damageReduction); // Apply reduction.
        currentHP -= actualDamage; // Subtract damage.
        currentHP = Mathf.Max(0f, currentHP); // Prevent negative HP.
        Debug.Log($"TakeDamage: HP before={currentHP + actualDamage}, after={currentHP}");
        onHealthChanged?.Invoke(); // Notify health change.

        if (currentHP <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            Debug.Log("Game Over");
            onDestroyed?.Invoke();
            // Enhanced: Explicitly destroy archers before deactivating to ensure cleanup.
            for (int i = 0; i < archers.Length; i++)
            {
                if (archers[i] != null)
                {
                    Destroy(archers[i]); // Destroy archers when tower is destroyed.
                    archers[i] = null;
                }
            }
            gameObject.SetActive(false); // Deactivate tower (hides it and stops updates).
        }
    }

    public bool Upgrade(float availableGold)
    {
        if (level >= maxLevel) return false; // Can't upgrade beyond max.

        float upgradeCost = upgradeCostBase * level * level; // Quadratic cost increase.
        if (availableGold < upgradeCost) return false;

        level++; // Increment level.
        maxHP += hpPerLevel; // Increase max HP.
        armor += armorPerLevel; // Increase armor.
        attackDamage += 1f; // Increase damage (unused currently).
        if (level >= 2)
        {
            int subTowerCount = level - 1; // Sub-towers based on level.
            onSubTowerUnlocked?.Invoke(subTowerCount);
            Debug.Log($"Mở khóa {subTowerCount} trụ phụ!");
        }

        UpdateArchers(); // Update archers (add more if level allows).
        onLevelUp?.Invoke(); // Notify level up.
        onHealthChanged?.Invoke(); // Notify health change (max HP increased).
        Debug.Log($"Nâng cấp! HP={currentHP}/{maxHP}, Armor={armor}, Level={level}, Damage={attackDamage}");
        return true; // Upgrade successful.
    }

    private void UpdateArchers()
    {
        int archerCount = Mathf.Min(level, 3); // Limit to level or 3 max.
        for (int i = 0; i < 3; i++)
        {
            if (i < archerCount && archers[i] == null)
            {
                if (archerSpawnPoints[i] == null) // Fixed: Null check before access to prevent UnassignedReferenceException.
                {
                    Debug.LogError($"Cannot spawn archer {i}: Spawn point is null! Assign in Inspector.");
                    continue; // Skip this archer to avoid crash.
                }

                Vector2 spawnPosition = (Vector2)archerSpawnPoints[i].position; // Cast to Vector2 for 2D.
                archers[i] = Instantiate(archerPrefab, spawnPosition, Quaternion.identity, transform); // Parent to tower so they deactivate together.
                archers[i].transform.localScale = new Vector3(0.5f, 0.5f, 1f); // Scale down archer.

                Archer archerScript = archers[i].GetComponent<Archer>(); // Get Archer component.
                if (archerScript != null)
                {
                    archerScript.Setup(level); // Initialize archer.
                }
                else
                {
                    Debug.LogError($"Archer script không tìm thấy trên cung thủ {i}!");
                }
            }
            else if (i >= archerCount && archers[i] != null)
            {
                Destroy(archers[i]); // Remove excess archers.
                archers[i] = null;
            }
        }
    }

    // Getter methods for external access (e.g., UI).
    public float GetCurrentHP() => currentHP;
    public float GetMaxHP() => maxHP;
    public float GetArmor() => armor;
    public int GetLevel() => level;
    public float GetUpgradeCost() => level >= maxLevel ? 0f : upgradeCostBase * level * level;
    public bool IsMaxLevel() => level >= maxLevel;
}