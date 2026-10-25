using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    Dictionary<GameObject, Queue<GameObject>> pools = new();
    Dictionary<GameObject, int> poolCount = new();

    [SerializeField] int initialSize = 3;
    [SerializeField] int expandSize = 2;
    [SerializeField] int maxSize = 30;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Critical hitboxes must not disappear when the visual pool limit is reached.
    public GameObject Get(GameObject prefab, bool essential = false)
    {
        if (!pools.ContainsKey(prefab))
            CreatePool(prefab, initialSize);

        if (pools[prefab].Count == 0)
        {
            if (essential)
                CreatePool(prefab, Mathf.Max(1, expandSize));
            else if (poolCount[prefab] < maxSize)
                CreatePool(prefab, Mathf.Min(Mathf.Max(1, expandSize), maxSize - poolCount[prefab]));
            else
                return null; // Safe to omit non-essential effects
        }

        var obj = pools[prefab].Dequeue();
        obj.SetActive(true);

        if (obj.TryGetComponent<IPoolable>(out var p))
            p.OnSpawn();

        return obj;
    }

    public void Release(GameObject prefab, GameObject obj)
    {
        if (!pools.ContainsKey(prefab))
        {
            Destroy(obj);
            return;
        }

        if (obj.TryGetComponent<IPoolable>(out var p))
            p.OnDespawn();

        obj.SetActive(false);

        // ★ 必ず PoolManager の子供に戻す
        obj.transform.SetParent(transform, false);

        pools[prefab].Enqueue(obj);
    }

    void CreatePool(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
            poolCount[prefab] = 0;
        }

        for (int i = 0; i < count; i++)
        {
            // ★ ここがポイント
            var obj = Instantiate(prefab, transform);

            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
            poolCount[prefab]++;
        }
    }
}
