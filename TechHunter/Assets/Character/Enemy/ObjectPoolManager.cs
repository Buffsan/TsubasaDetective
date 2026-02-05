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

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
            CreatePool(prefab, initialSize);

        if (pools[prefab].Count == 0)
        {
            if (poolCount[prefab] < maxSize)
                CreatePool(prefab, expandSize);
            else
                return null; // 上限超え → 演出省略
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
