using UnityEngine;
using System.Collections;

public class PoolAutoReturn : MonoBehaviour
{
    Coroutine timer;
    ObjectPoolManager pool;
    GameObject prefabKey;

    // 寿命指定あり
    public void StartLife(
        ObjectPoolManager pool,
        GameObject prefabKey,
        float lifeTime)
    {
        this.pool = pool;
        this.prefabKey = prefabKey;

        // 以前のタイマーが残っていた場合に備える
        if (timer != null)
        {
            StopCoroutine(timer);
            timer = null;
        }

        if (lifeTime <= 0f) return; // 寿命なし

        timer = StartCoroutine(LifeTimer(lifeTime));
    }

    IEnumerator LifeTimer(float time)
    {
        yield return new WaitForSeconds(time);

        // すでに非アクティブなら何もしない
        if (!gameObject.activeSelf) yield break;

        pool.Release(prefabKey, gameObject);
    }

    void OnDisable()
    {
        if (timer != null)
        {
            StopCoroutine(timer);
            timer = null;
        }
    }
}
