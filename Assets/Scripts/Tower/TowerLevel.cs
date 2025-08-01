using UnityEngine;

[System.Serializable]
public class TowerLevel
{
    public int damage;
    public float attackSpeed;
    public float range;
    public int upgradeCost;
    public int requiredXPToNextLevel;
    public GameObject visualPrefab;
}
