using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カーソル方向を中心とした扇形内に、上向きの剣をランダムな位置・時刻で隆起させる。
/// 10本を超えると一部を3本分の強化剣にまとめ、通常剣も残す。
/// </summary>
public class Skill_PD_SwordContract_Spawner : MonoBehaviour
{
    [Header("剣のPrefab")]
    [SerializeField] private GameObject SwordPrefab;
    [SerializeField] private GameObject StrongSwordPrefab;

    [Header("圧縮設定")]
    [SerializeField, Min(0)] private int CompressionStartCount = 10;
    [SerializeField, Min(2)] private int StrongSwordValue = 3;
    [SerializeField, Range(0f, 1f)] private float NormalSwordReserveRate = 0.25f;

    [Header("扇状の生成範囲")]
    [SerializeField, Min(0f)] private float MinDistance = 1f;
    [SerializeField, Min(0f)] private float MaxDistance = 7f;
    [SerializeField, Range(0f, 180f)] private float FanAngle = 90f;

    [Header("剣の向き（上方向を基準とした傾き）")]
    [SerializeField, Range(0f, 90f)] private float RandomTiltAngle = 12f;

    [Header("生成タイミング")]
    [SerializeField, Min(0f)] private float TotalSpawnTime = 1f;
    [SerializeField, Min(0f)] private float SwordLifeTime = 2f;

    private Vector2 direction = Vector2.right;
    private int swordCount;
    private bool initialized;

    public void Initialize(Vector2 spawnDirection, int count)
    {
        direction = spawnDirection.sqrMagnitude > 0f ? spawnDirection.normalized : Vector2.right;
        swordCount = Mathf.Max(0, count);
        initialized = true;
    }

    private void Start()
    {
        if (!initialized)
        {
            Debug.LogWarning("Skill_PD_SwordContract_Spawner: Initializeされていません。");
            Destroy(gameObject);
            return;
        }
        StartCoroutine(SpawnSwords());
    }

    private IEnumerator SpawnSwords()
    {
        int normalCount = swordCount;
        int strongCount = 0;

        if (swordCount > CompressionStartCount && StrongSwordPrefab != null)
        {
            int value = Mathf.Max(2, StrongSwordValue);
            // 総本数の一部を通常剣として確保し、残りをまとめる。
            int reservedNormal = Mathf.CeilToInt(swordCount * NormalSwordReserveRate);
            strongCount = (swordCount - reservedNormal) / value;
            normalCount = swordCount - strongCount * value;
        }

        List<GameObject> prefabs = new List<GameObject>(normalCount + strongCount);
        for (int i = 0; i < normalCount; i++) prefabs.Add(SwordPrefab);
        for (int i = 0; i < strongCount; i++) prefabs.Add(StrongSwordPrefab);

        // 剣の種類と生成順を混ぜる。
        for (int i = prefabs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = prefabs[i];
            prefabs[i] = prefabs[j];
            prefabs[j] = temp;
        }

        int n = prefabs.Count;
        if (n == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        // 時刻も独立してランダム抽選し昇順に並べる。
        // これにより全剣の生成予定時刻が0～TotalSpawnTime内になる。
        List<float> spawnTimes = new List<float>(n);
        for (int i = 0; i < n; i++)
            spawnTimes.Add(Random.Range(0f, TotalSpawnTime));
        spawnTimes.Sort();

        float startTime = Time.time;
        for (int i = 0; i < n; i++)
        {
            float remaining = startTime + spawnTimes[i] - Time.time;
            if (remaining > 0f) yield return new WaitForSeconds(remaining);
            SpawnSword(prefabs[i]);
        }

        Destroy(gameObject);
    }

    private void SpawnSword(GameObject prefab)
    {
        if (prefab == null) return;

        float angle = Random.Range(-FanAngle * 0.5f, FanAngle * 0.5f);
        Vector2 randomDirection = Quaternion.Euler(0f, 0f, angle) * direction;

        float min = Mathf.Max(0f, MinDistance);
        float max = Mathf.Max(min, MaxDistance);
        float distance = Mathf.Sqrt(Random.Range(min * min, max * max));
        Vector2 position = (Vector2)transform.position + randomDirection * distance;

        // カーソルには回転させず、真上を基準に微妙に傾ける。
        float tilt = Random.Range(-RandomTiltAngle, RandomTiltAngle);
        GameObject sword = Instantiate(prefab, position, Quaternion.Euler(0f, 0f, tilt));

        if (SwordLifeTime > 0f) Destroy(sword, SwordLifeTime);
    }
}
