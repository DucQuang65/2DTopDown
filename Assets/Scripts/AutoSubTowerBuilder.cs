using UnityEngine;
using UnityEngine.EventSystems;

public class AutoSubTowerBuilder : MonoBehaviour
{
    [Header("Tham chiếu")]
    public GameObject dragTargetUI;            // UI có gắn BuildingPlacer
    public GameObject housePrefabToPlace;      // Prefab SubTower muốn xây
    public int cost = 100;

    private bool hasTriggered = false;

    void Start()
    {
        GoldManager.Instance.OnGoldChanged += HandleGoldChanged;
    }

    void HandleGoldChanged(int currentGold)
    {
        if (!hasTriggered && currentGold >= cost)
        {
            if (GoldManager.Instance.SpendGold(cost))
            {
                var placer = dragTargetUI.GetComponent<BuildingPlacer>();
                placer.housePrefab = housePrefabToPlace;

                // Tự động kích hoạt bắt đầu kéo thả
                ExecuteEvents.Execute<IBeginDragHandler>(
                    dragTargetUI,
                    new PointerEventData(EventSystem.current),
                    ExecuteEvents.beginDragHandler
                );

                hasTriggered = true;  // Chỉ kích hoạt 1 lần
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
