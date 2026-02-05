using UnityEngine;
using System.Collections.Generic;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool instance;
    public GameObject prefab;
    public int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
            obj.transform.SetParent(transform);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // ë´ÇËÇ»ÇØÇÍÇŒí«â¡ê∂ê¨ÅiÇ®çDÇ›Åj
        GameObject newObj = Instantiate(prefab, transform);
        newObj.transform.SetParent(transform);
        return newObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
