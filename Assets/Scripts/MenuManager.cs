using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject iconPrefab;
    public Transform contentPanel;

    [Header("House Data")]
    public Sprite[] houseSprites;
    public GameObject[] housePrefabs;

    void Start()
    {
        for (int i = 0; i < houseSprites.Length; i++)
        {
            GameObject icon = Instantiate(iconPrefab, contentPanel);
            icon.GetComponent<Image>().sprite = houseSprites[i];

            var drag = icon.GetComponent<DraggableItem>();
            drag.prefabToSpawn = housePrefabs[i];
        }
    }
}
