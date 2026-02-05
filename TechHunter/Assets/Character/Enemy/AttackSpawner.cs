using UnityEngine;

public class AttackSpawner : MonoBehaviour
{
    public static AttackSpawner Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 攻撃・エフェクトを生成する
    /// </summary>
    public void Spawn(
     GameObject attackPrefab,
     Vector3 pos,
     Quaternion rot,
     float lifeTime,
     Transform parent = null)
    {
        // プールから取得
        var obj = ObjectPoolManager.Instance.Get(attackPrefab);
        if (obj == null) return;

        // 親 → ワールド座標を維持
        obj.transform.SetParent(parent, false);

        // 位置・回転を反映
        obj.transform.SetPositionAndRotation(pos, rot);

        // PoolAutoReturn がなければ付ける
        if (!obj.TryGetComponent<PoolAutoReturn>(out var autoReturn))
        {
            autoReturn = obj.AddComponent<PoolAutoReturn>();
        }

        // 寿命開始
        autoReturn.StartLife(
            ObjectPoolManager.Instance,
            attackPrefab,
            lifeTime);
    }
}
