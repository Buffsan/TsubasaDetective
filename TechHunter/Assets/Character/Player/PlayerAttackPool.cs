using System.Collections.Generic;
using UnityEngine;

public class AttackPool : MonoBehaviour
{
    public static AttackPool Instance;

    [SerializeField] GameObject pooledAttackPrefab; // 汎用の攻撃判定インスタンス（Circle/Box両方持ってても良い）
    [SerializeField] int initialCount = 20;

    Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < initialCount; i++)
        {
            var go = Instantiate(pooledAttackPrefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject Get(GameObject templatePrefab, Vector3 pos, object rotOrDir, float lifetime)
    {
        Quaternion rot;

        // --------- 型判定して切り替え ---------
        if (rotOrDir is Quaternion q)
        {
            rot = q;
        }
        else if (rotOrDir is Vector2 dir)
        {
            rot = Quaternion.LookRotation(Vector3.forward, dir.normalized);
        }
        else if (rotOrDir is Vector3 dir3)
        {
            rot = Quaternion.LookRotation(Vector3.forward, dir3.normalized);
        }
        else
        {
            Debug.LogWarning("Get: rotOrDir は Quaternion か Vector2/Vector3 で渡してください");
            rot = Quaternion.identity;
        }
        GameObject instance = (pool.Count > 0) ? pool.Dequeue() : Instantiate(pooledAttackPrefab, transform);

        instance.transform.SetParent(transform);
        instance.transform.SetPositionAndRotation(pos, rot);

        // コライダーコピー
        CopyCollider2D(templatePrefab, instance);

        // パラメータコピー
        var tAttack = templatePrefab.GetComponent<PlayerAttack>();
        var iAttack = instance.GetComponent<PlayerAttack>();
        if (tAttack != null )
        {
            iAttack.skillcarddata = tAttack.skillcarddata;
            iAttack.weapondata = tAttack.weapondata;
            iAttack.isCriticulUse = tAttack.isCriticulUse;
        }
        
        instance.SetActive(true);

        // 自動消滅用スクリプトをセット
        var despawn = instance.GetComponent<AttackDespawn>();
        if (despawn == null) despawn = instance.AddComponent<AttackDespawn>();
        despawn.Init(lifetime, this); // プールに戻す

        return instance;
    }

    public void Release(GameObject instance)
    {
        instance.SetActive(false);
        pool.Enqueue(instance);
    }

    // ---------- コピー処理（2D） ----------
    void CopyCollider2D(GameObject sourcePrefab, GameObject targetInstance)
    {
        // CircleCollider2D を優先してコピー
        var srcCircle = sourcePrefab.GetComponent<CircleCollider2D>();
        var srcBox = sourcePrefab.GetComponent<BoxCollider2D>();

        // ターゲットに Circle / Box があるかチェック
        var tgtCircle = targetInstance.GetComponent<CircleCollider2D>();
        var tgtBox = targetInstance.GetComponent<BoxCollider2D>();

        targetInstance.transform.localScale = sourcePrefab.transform.localScale;

        // まず両方 disable にしておく（余計な当たりを防ぐ）
        if (tgtCircle != null) tgtCircle.enabled = false;
        if (tgtBox != null) tgtBox.enabled = false;

        if (srcCircle != null && tgtCircle != null)
        {
            // Circle -> Circle コピー
            tgtCircle.radius = srcCircle.radius;
            tgtCircle.offset = srcCircle.offset;
            tgtCircle.isTrigger = srcCircle.isTrigger;
            tgtCircle.usedByEffector = srcCircle.usedByEffector;
            tgtCircle.enabled = true;
            return;
        }

        if (srcBox != null && tgtBox != null)
        {
            // Box -> Box コピー
            tgtBox.size = srcBox.size;
            tgtBox.offset = srcBox.offset;
            tgtBox.isTrigger = srcBox.isTrigger;
            tgtBox.usedByEffector = srcBox.usedByEffector;
            tgtBox.enabled = true;
            return;
        }

        // もしテンプレに別の Collider2D が付いていたら個別に処理追加
    }
}
