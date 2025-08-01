using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Tower/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName = "New Tower";
    public Sprite towerSprite; // Sprite hiển thị của tháp
    public GameObject towerPrefab; // Prefab của tháp để spawn
    public List<TowerLevel> levels; // Danh sách các thông số cho từng cấp độ
}

// Đánh dấu Serializable để Unity có thể lưu trữ dữ liệu lớp này
