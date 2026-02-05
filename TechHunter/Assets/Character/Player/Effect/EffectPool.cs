using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance;

    [SerializeField] GameObject pooledEffectPrefab;
    [SerializeField] int initialCount = 20;

    Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialCount; i++)
        {
            var go = Instantiate(pooledEffectPrefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    /// <summary>
    /// エフェクトを取得して再生
    /// </summary>
    public GameObject Get(
        GameObject templatePrefab,
        Vector3 pos,
        object rotOrDir,
        float lifetime = -1f,
        Transform followTarget = null)
    {
        Quaternion rot;

        // 回転の型に応じて判定
        if (rotOrDir is Quaternion q)
            rot = q;
        else if (rotOrDir is Vector2 v2)
            rot = Quaternion.LookRotation(Vector3.forward, v2.normalized);
        else if (rotOrDir is Vector3 v3)
            rot = Quaternion.LookRotation(Vector3.forward, v3.normalized);
        else
            rot = Quaternion.identity;

        // 取得
        GameObject instance = (pool.Count > 0)
            ? pool.Dequeue()
            : Instantiate(pooledEffectPrefab);

        // parent があるなら設定。無ければ外す
       

        // 位置・回転
        instance.transform.SetPositionAndRotation(pos, rot);

        // ParticleSystem の設定をテンプレからコピー
        CopyParticleSystems(templatePrefab, instance);

        instance.SetActive(true);

        var ps = instance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear();
            ps.Play();
        }

        // lifetime が無い場合は自動計算
        float autoLife = lifetime;

        if (lifetime <= 0 && ps != null)
            autoLife = ps.main.duration + ps.main.startLifetime.constantMax;

        // 自動消滅
        var despawn = instance.GetComponent<EffectDespawn>();
        if (despawn == null) despawn = instance.AddComponent<EffectDespawn>();
        despawn.Init(autoLife, this);

        // ---- 追従設定（敵が死んでも安全） ----
        if (followTarget != null)
        {
            var follow = instance.GetComponent<AttachFollowTarget>();
            if (follow == null) follow = instance.AddComponent<AttachFollowTarget>();
            follow.Init(followTarget);
        }

        return instance;
    }

    /// <summary>
    /// 使い終わったエフェクトをプールに戻す
    /// </summary>
    public void Release(GameObject instance)
    {
        instance.SetActive(false);

        // プール管理下に戻す
        instance.transform.SetParent(transform);

        pool.Enqueue(instance);
    }

    // === ParticleSystem の完全コピー ===
    void CopyParticleSystems(GameObject template, GameObject instance)
    {
        var srcSystems = template.GetComponentsInChildren<ParticleSystem>(true);
        var dstSystems = instance.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < Mathf.Min(srcSystems.Length, dstSystems.Length); i++)
        {
            CopyParticleSystem(srcSystems[i], dstSystems[i]);
        }
    }

    void CopyParticleSystem(ParticleSystem src, ParticleSystem dst)
    {
        // --- Main ---
        var sm = src.main;
        var dm = dst.main;
        dm.duration = sm.duration;
        dm.loop = sm.loop;
        dm.prewarm = sm.prewarm;
        dm.startDelay = sm.startDelay;
        dm.startLifetime = sm.startLifetime;
        dm.startSpeed = sm.startSpeed;
        dm.startSize = sm.startSize;
        dm.startColor = sm.startColor;
        dm.gravityModifier = sm.gravityModifier;
        dm.simulationSpace = sm.simulationSpace;
        dm.simulationSpeed = sm.simulationSpeed;
        dm.scalingMode = sm.scalingMode;
        dm.maxParticles = sm.maxParticles;

        // --- Emission ---
        var se = src.emission;
        var de = dst.emission;
        de.enabled = se.enabled;
        de.rateOverTime = se.rateOverTime;
        de.rateOverDistance = se.rateOverDistance;

        // Bursts
        dst.emission.SetBursts(System.Array.Empty<ParticleSystem.Burst>());

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[se.burstCount];
        se.GetBursts(bursts);
        de.SetBursts(bursts);


        // --- Shape ---
        var ss = src.shape;
        var ds = dst.shape;
        ds.enabled = ss.enabled;
        ds.shapeType = ss.shapeType;
        ds.radius = ss.radius;
        ds.angle = ss.angle;
        ds.length = ss.length;
        ds.scale = ss.scale;

        // --- Velocity over Lifetime ---
        var sv = src.velocityOverLifetime;
        var dv = dst.velocityOverLifetime;
        dv.enabled = sv.enabled;
        dv.x = sv.x;
        dv.y = sv.y;
        dv.z = sv.z;
        dv.space = sv.space;

        // --- Color over Lifetime ---
        var scol = src.colorOverLifetime;
        var dcol = dst.colorOverLifetime;
        dcol.enabled = scol.enabled;
        dcol.color = scol.color;

        // --- Size over Lifetime ---
        var ssz = src.sizeOverLifetime;
        var dsz = dst.sizeOverLifetime;
        dsz.enabled = ssz.enabled;
        dsz.size = ssz.size;

        // --- Rotation over Lifetime ---
        var srot = src.rotationOverLifetime;
        var drot = dst.rotationOverLifetime;
        drot.enabled = srot.enabled;
        drot.x = srot.x;
        drot.y = srot.y;
        drot.z = srot.z;

        // --- Noise ---
        var sn = src.noise;
        var dn = dst.noise;
        dn.enabled = sn.enabled;
        dn.strength = sn.strength;
        dn.frequency = sn.frequency;
        dn.octaveCount = sn.octaveCount;

        // --- Trails ---
        var st = src.trails;
        var dt = dst.trails;
        dt.enabled = st.enabled;
        dt.mode = st.mode;
        dt.ratio = st.ratio;
        dt.lifetime = st.lifetime;


        // --- Start Rotation ---
        dm.startRotation = sm.startRotation;

#if UNITY_2020_2_OR_NEWER
        dm.startRotation3D = sm.startRotation3D;
        dm.startRotationX = sm.startRotationX;
        dm.startRotationY = sm.startRotationY;
        dm.startRotationZ = sm.startRotationZ;
#endif

        // --- Renderer ---
        var sr = src.GetComponent<ParticleSystemRenderer>();
        var dr = dst.GetComponent<ParticleSystemRenderer>();
        if (sr && dr)
        {
            dr.sharedMaterial = sr.sharedMaterial;
            dr.renderMode = sr.renderMode;
            dr.mesh = sr.mesh;
            dr.sortingOrder = sr.sortingOrder;
            dr.sortingLayerID = sr.sortingLayerID;
            dr.maxParticleSize = sr.maxParticleSize;
            dr.alignment = sr.alignment;
        }
    }
}
