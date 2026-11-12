using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Player-damageable cage that releases existing enemy prefabs as StageAllies.
/// It is deliberately NOT an EnemyBase, so AI target scans and AllEnemy/kill counts ignore it.
/// Put this on the object carrying an EnemyArea-tagged Collider2D.
/// </summary>
[DisallowMultipleComponent]
public sealed class AllyReleaseCage : MonoBehaviour, IDamageable
{
    [Header("敵と共通のステータス ScriptableObject")]
    [Tooltip("通常の敵と同じcharadataを指定します。MAXHPとATKDFを使用し、GOLD/XPは破壊時に付与しません。")]
    [SerializeField] charadata enemyData;

    [Header("破壊時に解放する既存の敵セット（EnemySpawnBase.enemyListから取得）")]
    [SerializeField] List<EnemySpawnGroup> releaseEnemies = new List<EnemySpawnGroup>();
    [Tooltip("任意。既存EnemySpawnDataをセットした場合、allyspawnGroupを優先し、空ならenemyspawnGroupを使用します。")]
    [SerializeField] EnemySpawnData releasePreset;
    [Tooltip("任意。未設定ならALL_SystemManagerから取得します。")]
    [SerializeField] EnemySpawnBase spawnDatabase;

    [Header("散開演出")]
    [SerializeField, Min(0f)] float minimumScatterRadius = 0.8f;
    [SerializeField, Min(0f)] float maximumScatterRadius = 3f;
    [SerializeField, Min(0f)] float scatterDuration = 0.35f;
    [Tooltip("床タイル外への飛び出しを可能な限り防止します。")]
    [SerializeField] bool keepOnFloorTiles = true;
    [SerializeField] Transform releaseOrigin;

    [Header("任意の演出")]
    [SerializeField] GameObject breakEffect;
    [SerializeField] GameObject eachAllyEffect;

    float hp;
    bool destroyed;
    bool hasValidData;
    public charadata EnemyData => enemyData;
    public float CurrentHP => hp;
    public float MaximumHP => enemyData != null ? enemyData.MAXHP : 0f;
    public bool IsBroken => destroyed;

    void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError("檻に敵用ScriptableObject（charadata）が設定されていません。", this);
            return;
        }
        if (enemyData.MAXHP <= 0f)
        {
            Debug.LogError("檻のcharadata.MAXHPは0より大きい値を指定してください。", this);
            return;
        }

        // 敵と同様にcharadata.MAXHPを初期HPとして使用。
        // HPの残量は個体ごとに管理し、共有SO自体は変更しない。
        hp = enemyData.MAXHP;
        hasValidData = true;
    }

    // PlayerAttackはEnemyAreaタグのIDamageableを呼ぶ。敵のAIとEnemyAttackは
    // charaDamage/EnemyBaseを対象にするため、この檻をターゲットにしない。
    public void Damage(float attack, float stagger, float confusion, bool critical)
    {
        if (destroyed || !hasValidData || attack <= 0f) return;
        // charaDamage.receiveDamageと同じ「攻撃力-防御、最低1ダメージ」。
        hp -= Mathf.Max(1f, attack - enemyData.ATKDF);
        if (hp <= 0f) Death();
    }

    // 状態異常は檻には適用しない。スキル自体のダメージは通常どおり通る。
    public void SpawnBuff(StatusEffectAttack buffData) { }

    [ContextMenu("Break Cage (Play Mode)")]
    public void Death()
    {
        if (destroyed) return;
        destroyed = true;

        // 破壊した瞬間に二重ヒットしないよう当たり判定を停止。
        foreach (Collider2D hitbox in GetComponentsInChildren<Collider2D>(true))
            hitbox.enabled = false;

        Vector3 origin = releaseOrigin != null ? releaseOrigin.position : transform.position;
        if (breakEffect != null) Instantiate(breakEffect, origin, Quaternion.identity);

        ReleaseAllies(origin);
        Destroy(gameObject); // 檻は敵リストに登録されず、報酬も出さない。
    }

    void ReleaseAllies(Vector3 origin)
    {
        EnemySpawnBase database = spawnDatabase;
        if (database == null && ALL_SystemManager.Instance != null)
            database = ALL_SystemManager.Instance.enemySpawnBASE;
        if (database == null)
        {
            Debug.LogError("檻の解放に必要なEnemySpawnBaseがありません。", this);
            return;
        }

        List<EnemySpawnGroup> groups = new List<EnemySpawnGroup>();
        if (releasePreset != null)
        {
            List<EnemySpawnGroup> presetGroups =
                releasePreset.allyspawnGroup != null && releasePreset.allyspawnGroup.Count > 0
                    ? releasePreset.allyspawnGroup : releasePreset.enemyspawnGroup;
            if (presetGroups != null) groups.AddRange(presetGroups);
        }
        if (releaseEnemies != null) groups.AddRange(releaseEnemies);

        int total = 0;
        foreach (EnemySpawnGroup group in groups)
            if (group != null) total += Mathf.Max(0, group.SpawnNumber);
        if (total == 0) return;

        Tilemap floor = database.Seve_TileMap_C != null
            ? database.Seve_TileMap_C.FloorMap : null;
        float startAngle = Random.Range(0f, Mathf.PI * 2f);
        int index = 0;

        foreach (EnemySpawnGroup group in groups)
        {
            if (group == null || group.SpawnNumber <= 0) continue;
            GameObject prefab = database.GetEnemyPrefab(group.enemyType);
            if (prefab == null)
            {
                Debug.LogWarning("檻に登録した敵のPrefabが見つかりません: " + group.enemyType, this);
                index += group.SpawnNumber;
                continue;
            }

            for (int i = 0; i < group.SpawnNumber; i++, index++)
            {
                Vector3 destination = FindScatterPosition(
                    origin, floor, index, total, startAngle);

                GameObject instance = Instantiate(prefab, origin, Quaternion.identity);
                EnemyBase ally = instance.GetComponent<EnemyBase>();
                if (ally == null)
                {
                    Debug.LogError("檻から出すPrefabのルートにEnemyBaseがありません: " + prefab.name, instance);
                    Destroy(instance);
                    continue;
                }

                ally.BecomeStageAlly();
                if (!ally.IsStageAlly)
                {
                    Debug.LogError("味方化できないPrefabです: " + prefab.name, instance);
                    Destroy(instance);
                    continue;
                }

                if (eachAllyEffect != null)
                    Instantiate(eachAllyEffect, origin, Quaternion.identity);

                // AIを短時間停止し、檻の中心から円状に飛び出す。
                if (scatterDuration > 0f &&
                    (destination - origin).sqrMagnitude > 0.001f)
                {
                    AllyCageScatterMotion motion = instance.AddComponent<AllyCageScatterMotion>();
                    motion.Begin(destination, scatterDuration);
                }
                else
                {
                    instance.transform.position = destination;
                }
            }
        }
    }

    Vector3 FindScatterPosition(Vector3 origin, Tilemap floor, int index,
        int total, float startAngle)
    {
        float min = Mathf.Max(0f, minimumScatterRadius);
        float max = Mathf.Max(min, maximumScatterRadius);
        float angle = startAngle + index * (Mathf.PI * 2f / Mathf.Max(1, total));
        float radius = Mathf.Lerp(min, max, (index % 3 + 1f) / 3f);

        for (int attempt = 0; attempt < 12; attempt++)
        {
            float a = angle + attempt * 0.5f;
            float r = Mathf.Max(0.1f, radius - attempt * 0.1f);
            Vector3 destination = origin + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);

            if (!keepOnFloorTiles || floor == null ||
                floor.HasTile(floor.WorldToCell(destination)))
                return destination;
        }

        // 地形の端などで床がない場合は、檻の位置を利用する。
        return origin;
    }
}
