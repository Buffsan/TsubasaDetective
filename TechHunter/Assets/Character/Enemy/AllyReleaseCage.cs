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
    [Tooltip("出現先でのポップアップ演出時間。移動せず、決定した安全な地点で再生します。")]
    [SerializeField, Min(0f)] float scatterDuration = 0.35f;
    [Tooltip("出現地点とステージ壁・穴・通行不可コライダーとの最低距離。大きな味方は増やしてください。")]
    [SerializeField, Min(0f)] float obstacleClearanceRadius = 0.35f;
    [Tooltip("解放された味方同士の最低間隔（タイル中心間）。")]
    [SerializeField, Min(0f)] float minimumAllySpacing = 1f;
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

    // 檻の破壊時のみ一度実行。通常の敵の生成Prefabはそのまま味方に転用する。
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

        List<GameObject> releasePrefabs = new List<GameObject>();
        foreach (EnemySpawnGroup group in groups)
        {
            if (group == null || group.SpawnNumber <= 0) continue;
            GameObject prefab = database.GetEnemyPrefab(group.enemyType);
            if (prefab == null)
            {
                Debug.LogWarning("檻に登録した敵のPrefabが見つかりません: " + group.enemyType, this);
                continue;
            }
            for (int i = 0; i < group.SpawnNumber; i++)
                releasePrefabs.Add(prefab);
        }

        if (releasePrefabs.Count == 0) return;

        // FloorMapではなく、歩行可能な領域を表すStandFloorのみを使用。
        Tilemap standFloor = database.Seve_TileMap_C != null
            ? database.Seve_TileMap_C.StandFloor : null;

        // validTilesが空の場合、必ず檻の座標（origin + Vector3.zero）に配置する。
        List<Vector3> validTiles = FindStandFloorSpawnPoints(standFloor, origin);
        List<Vector3> usedPositions = new List<Vector3>();
        int tileCursor = 0;

        for (int index = 0; index < releasePrefabs.Count; index++)
        {
            Vector3 spawnPosition = origin;
            while (tileCursor < validTiles.Count)
            {
                Vector3 candidate = validTiles[tileCursor++];
                bool tooClose = false;
                float spacingSqr = minimumAllySpacing * minimumAllySpacing;
                foreach (Vector3 used in usedPositions)
                {
                    if (((Vector2)(candidate - used)).sqrMagnitude < spacingSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                usedPositions.Add(candidate);
                spawnPosition = candidate;
                break;
            }

            // 散開時に壁を横切らないよう、最初から安全なStandFloor地点に生成。
            // 候補が無い／使い切った場合だけ檻の元座標を使う。
            GameObject prefab = releasePrefabs[index];
            GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
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
                Instantiate(eachAllyEffect, spawnPosition, Quaternion.identity);

            // 飛び出し表現は位置を補間せず、着地点で拡大して現れる演出にする。
            // コライダーを無効化した直線移動で壁をすり抜ける問題を防ぐ。
            if (scatterDuration > 0f)
            {
                AllyCageScatterMotion motion = instance.AddComponent<AllyCageScatterMotion>();
                motion.Begin(scatterDuration);
            }
        }
    }

    // 檻から最大半径内のStandFloorタイルを探す。
    // 有効タイルが見つからない場合は空リストを返し、解放時に檻のoriginへフォールバック。
    List<Vector3> FindStandFloorSpawnPoints(Tilemap standFloor, Vector3 origin)
    {
        List<Vector3> candidates = new List<Vector3>();
        if (standFloor == null)
        {
            Debug.LogWarning("StandFloorが設定されていません。檻の位置（オフセット0）に解放します。", this);
            return candidates;
        }

        float minimum = Mathf.Max(0f, minimumScatterRadius);
        float maximum = Mathf.Max(minimum, maximumScatterRadius);
        float minSqr = minimum * minimum;
        float maxSqr = maximum * maximum;

        // 候補は必ずStandFloorの「実在するタイル」の中心に限定。
        // ワールド座標の距離で絞るので、Gridセルサイズが1以外でも正しく動作する。
        foreach (Vector3Int cell in standFloor.cellBounds.allPositionsWithin)
        {
            if (!standFloor.HasTile(cell)) continue;

            Vector3 tileCenter = standFloor.GetCellCenterWorld(cell);
            tileCenter.z = origin.z;
            float distanceSqr = ((Vector2)(tileCenter - origin)).sqrMagnitude;
            if (distanceSqr < minSqr || distanceSqr > maxSqr) continue;

            // タイル上でもStage_Wall等のコライダーと重なる場所は除外。
            if (OverlapsBlockingStageCollider(tileCenter)) continue;

            candidates.Add(tileCenter);
        }

        // 同じタイルに重複して出ないよう、候補をランダム順で1回ずつ使用。
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int swap = Random.Range(0, i + 1);
            Vector3 temp = candidates[i];
            candidates[i] = candidates[swap];
            candidates[swap] = temp;
        }

        if (candidates.Count == 0)
            Debug.LogWarning("散開範囲内に利用可能なStandFloorタイルがありません。檻の位置（オフセット0）に解放します。", this);

        return candidates;
    }

    bool OverlapsBlockingStageCollider(Vector3 position)
    {
        float radius = obstacleClearanceRadius;
        if (radius <= 0f) return false;

        // ステージ壁・穴・非歩行エリアはPrefabのキャラColliderと独立して確認。
        // 他の味方・敵のColliderは出現を妨げない（敵に囲まれた檻を解放できる）。
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            if (hit.CompareTag("Stage_Wall") ||
                hit.CompareTag("Stage_Hole") ||
                hit.CompareTag("Stage_NotFloor"))
                return true;
        }
        return false;
    }
}
