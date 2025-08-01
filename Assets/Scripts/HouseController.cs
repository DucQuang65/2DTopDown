using System.Collections.Generic;
using UnityEngine;

public class HouseController : MonoBehaviour
{
    public GameObject soldierPrefab;
    public int maxSoldiers = 5;
    public float spawnRadius = 2f;

    private List<GameObject> soldiers = new List<GameObject>();

    private Health parentSubTowerHealth;

    void Awake()
    {
        parentSubTowerHealth = GetComponent<Health>();
    }

    void Start()
    {
        SpawnSoldiers(maxSoldiers);
    }

    void Update()
    {
        soldiers.RemoveAll(s => s == null);

        int need = maxSoldiers - soldiers.Count;
        if (need > 0)
            SpawnSoldiers(need);
    }

    void SpawnSoldiers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(rnd.x, rnd.y, 0f);

            GameObject s = Instantiate(soldierPrefab, spawnPos, Quaternion.identity, transform);
            soldiers.Add(s);
        }
    }
}
