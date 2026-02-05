using System.Collections.Generic;
using UnityEngine;

public class CoinPool : MonoBehaviour
{
    public static CoinPool Instance;

    [SerializeField] GameObject coinPrefab;
    [SerializeField] int initialCount = 50;

    Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        // 事前に生成してプールに投入（超重要）
        for (int i = 0; i < initialCount; i++)
        {
            GameObject obj = Instantiate(coinPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
            obj.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// コインをプールから取得
    /// </summary>
    public GameObject GetCoin(Vector3 pos)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            // 予備が無ければ増築
            obj = Instantiate(coinPrefab);
            obj.transform.SetParent(transform);
        }

        obj.transform.position = pos;
        return obj;
    }

    /// <summary>
    /// コインをプールに戻す
    /// </summary>
    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
