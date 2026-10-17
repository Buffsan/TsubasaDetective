using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 傀儡子専用。通常の岩針生成には影響させない。
public class CG_Puppeteer_NeedlePool : MonoBehaviour
{
    readonly Queue<GameObject> available = new Queue<GameObject>();
    GameObject needlePrefab;
    int createdCount;

    public void Initialize(GameObject prefab, int initialCount)
    {
        needlePrefab = prefab;
        for (int i = 0; i < initialCount; i++)
            available.Enqueue(CreateNeedle());
    }

    GameObject CreateNeedle()
    {
        GameObject needle = Instantiate(needlePrefab, transform);
        CG_Puppeteer_Needle script = needle.GetComponent<CG_Puppeteer_Needle>();
        script.AttackType = 1;
        needle.SetActive(false);
        createdCount++;
        return needle;
    }

    public void Spawn(Vector2 position, EnemyBase owner)
    {
        GameObject needle = available.Count > 0 ? available.Dequeue() : CreateNeedle();
        needle.transform.SetPositionAndRotation(position, Quaternion.identity);
        CG_Puppeteer_Needle script = needle.GetComponent<CG_Puppeteer_Needle>();
        script.enemyAttack.enemyBase = owner;
        script.ResetForPool();
        needle.SetActive(true);
        StartCoroutine(ReturnAfterDelay(needle, 5f));
    }

    IEnumerator ReturnAfterDelay(GameObject needle, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (needle == null) yield break;
        needle.SetActive(false);
        available.Enqueue(needle);
    }
}
