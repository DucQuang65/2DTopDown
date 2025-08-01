using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class BuildingPlacer : MonoBehaviour,
                             IBeginDragHandler,
                             IDragHandler,
                             IEndDragHandler
{
    public string placerID;
    [Header("Prefabs & Zone")]
    public GameObject housePrefab;
    public BoxCollider2D buildZone;

    [Header("Overlap Detection")]
    public LayerMask houseLayerMask;

    [Header("Preview Colors")]
    public Color validColor = new Color(1f, 1f, 1f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Cost & UI")]
    public int baseCost = 50;
    public float currentCost;
    public TextMeshProUGUI costText;  // Gán Text hiển thị giá trong Inspector

    GameObject preview;
    SpriteRenderer previewSr;
    Collider2D previewCol;
    bool canPlace;
    private bool isInitialized = false;

    private void Start()
    {
        if (!isInitialized)
        {
            currentCost = baseCost;
            UpdateCostText();
            isInitialized = true;
        }
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (!GoldManager.Instance.HasEnoughGold(Mathf.CeilToInt(currentCost)))
        {
            // Không đủ tiền => không cho kéo đặt
            Debug.Log("Không đủ vàng để bắt đầu đặt SubTower!");
            return;
        }

        preview = Instantiate(housePrefab);
        previewSr = preview.GetComponent<SpriteRenderer>();
        previewSr.color = validColor;

        previewCol = preview.GetComponent<Collider2D>();
        if (previewCol != null)
            previewCol.isTrigger = true;
    }

    public void OnDrag(PointerEventData e)
    {
        if (preview == null) return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(e.position);
        wp.z = 0f;
        preview.transform.position = wp;

        bool inZone = buildZone.bounds.Contains(wp);

        bool overlap = false;
        if (previewCol != null)
        {
            var b = previewCol.bounds;
            var hits = Physics2D.OverlapBoxAll(
                b.center,
                b.size,
                0f,
                houseLayerMask
            );
            foreach (var h in hits)
            {
                if (h == previewCol) continue;
                overlap = true;
                break;
            }
        }

        canPlace = inZone && !overlap;
        previewSr.color = canPlace ? validColor : invalidColor;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (preview == null) return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(e.position);
        wp.z = 0f;

        if (canPlace && GoldManager.Instance.HasEnoughGold(Mathf.CeilToInt(currentCost)))
        {
            if (GoldManager.Instance.SpendGold(Mathf.CeilToInt(currentCost)))
            {
                enemySpawner.PlaceSubTower(housePrefab, wp);

                // Tăng giá 10%
                currentCost *= 1.1f;
                UpdateCostText();
            }
        }
        else
        {
            Debug.Log("Không đủ vàng để xây SubTower!");
        }

        Destroy(preview);
    }

    public void UpdateCostText()
    {
        if (costText != null)
        {
            costText.text = $"Gold: {Mathf.CeilToInt(currentCost)}";
        }
    }

    void OnDrawGizmosSelected() 
    {
        if (buildZone != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(buildZone.bounds.center, buildZone.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(buildZone.bounds.center, buildZone.bounds.size);
        }
    }
    public void SetCurrentCost(float cost)
    {
        currentCost = cost;
        UpdateCostText(); // cập nhật lại UI hiển thị giá vàng
    }
    public float GetCurrentCost()
    {
        return currentCost;
    }
    public void InitializeCost(float cost)
    {
        currentCost = cost;
        UpdateCostText();
        isInitialized = true;
    }
}
