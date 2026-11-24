using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Independent stage gimmick; no NPC or EnemySpawnData registration.</summary>
[DisallowMultipleComponent]
public sealed class StageCannonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cannonBase;
    [SerializeField] Transform turret;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform mountPoint;
    [SerializeField] Transform gaugeAnchor;
    [SerializeField] Camera aimCamera;
    [SerializeField] GameObject targetMarkerPrefab;
    [SerializeField] GameObject projectileVisualPrefab;
    [SerializeField] GameObject muzzleEffectPrefab;
    [SerializeField] GameObject explosionEffectPrefab;
    [SerializeField] AudioClip fireSE;
    [SerializeField] AudioClip explosionSE;

    [Header("Mount and reload")]
    [SerializeField, Min(0f)] float holdSeconds = 0.3f;
    [SerializeField, Min(0.01f)] float interactionDistance = 1.5f;
    [SerializeField, Min(0f)] float reloadSeconds = 10f;
    [SerializeField, Min(0f)] float invincibleSeconds = 3f;
    [SerializeField, Min(0.01f)] float escapeSpeedMultiplier = 2f;
    [SerializeField] InputActionReference optionalMountAction;
    [SerializeField] InputActionReference optionalFireAction;

    [Header("Camera and feedback")]
    [SerializeField, Min(1f)] float zoomMultiplier = 1.5f;
    [SerializeField, Min(0f)] float zoomSeconds = 0.3f;
    [SerializeField, Min(0f)] float restoreSeconds = 0.3f;
    [SerializeField, Min(0f)] float fireEffectSeconds = 0.5f;
    [SerializeField, Min(0f)] float recoilDistance = 0.45f;
    [SerializeField, Min(0f)] float shakeSeconds = 0.5f;
    [SerializeField, Min(0f)] float shakeStrength = 0.65f;
    [SerializeField, Min(0f)] float aimDegreesPerSecond = 0f;
    [SerializeField] float turretAngleOffset = 0f;
    [SerializeField, Min(0.05f)] float markerRefreshSeconds = 0.15f;
    [SerializeField] Vector2 markerOffset = new Vector2(0f, 0.9f);

    [Header("Weapon data (optional templates; attack is overridden below)")]
    [SerializeField] weapondata penetrationTemplate;
    [SerializeField] weapondata explosionTemplate;
    [SerializeField, Min(0f)] float penetrationAttack = 220f;
    [SerializeField, Min(0f)] float explosionAttack = 4100f;
    [SerializeField, Min(0.01f)] float projectileSpeed = 35f;
    [SerializeField, Min(0.01f)] float projectileDistance = 45f;
    [SerializeField, Min(0.01f)] float projectileRadius = 0.18f;
    [SerializeField, Min(0.01f)] float explosionRadius = 2.5f;
    [SerializeField] LayerMask enemyHitLayers = ~0;

    [Header("Prototype visuals (replace with art as needed)")]
    [SerializeField] bool generateFallbackVisuals = true;
    [SerializeField] bool generateFallbackCollider = true;
    [SerializeField] int cannonSortingOrder = 100;
    [SerializeField] int feedbackSortingOrder = 1000;
    [SerializeField, Min(0.1f)] float gaugeWidth = 2.6f;
    [SerializeField, Min(0.02f)] float gaugeHeight = 0.16f;

    enum State { Idle, Aiming, AwaitingEscape, Escaping }
    State state;
    static StageCannonController active;
    PlayerController owner;
    float holdElapsed, reloadRemaining, nextMarkerRefresh;
    bool holdConsumed;
    Vector2 aimDirection = Vector2.right;
    Vector3 turretRestLocalPosition, recoilLocalDirection;
    float recoilStart = -100f;
    Camera zoomCamera;
    float normalSize, zoomStart, zoomTarget, zoomElapsed, zoomDuration;
    bool zoomOwned, zoomRestoring;
    weapondata penetrationData, explosionData;
    Transform gaugeFill;
    SpriteRenderer gaugeFillRenderer;
    Sprite whiteSprite, ringSprite, diskSprite;
    Texture2D ringTexture, diskTexture;

    sealed class Shot
    {
        public PlayerController player;
        public Vector2 position, direction;
        public float remaining;
        public GameObject visual;
        public readonly HashSet<EnemyBase> penetrated = new HashSet<EnemyBase>();
    }

    sealed class Pulse
    {
        public GameObject visual;
        public SpriteRenderer renderer;
        public Vector3 from, to;
        public Color color;
        public float start, duration;
    }

    readonly List<Shot> shots = new List<Shot>();
    readonly List<Pulse> pulses = new List<Pulse>();
    readonly Dictionary<EnemyBase, GameObject> markers = new Dictionary<EnemyBase, GameObject>();
    readonly List<EnemyBase> staleMarkers = new List<EnemyBase>();

    public bool IsLoaded => reloadRemaining <= 0f;
    public float ReloadProgress => reloadSeconds <= 0f ? 1f :
        1f - Mathf.Clamp01(reloadRemaining / reloadSeconds);

    void Awake()
    {
        cannonBase = EnsureChild(cannonBase, "Base", transform, Vector3.zero);
        turret = EnsureChild(turret, "Turret", transform, Vector3.zero);
        firePoint = EnsureChild(firePoint, "FirePoint", turret, new Vector3(2.1f, 0f, 0f));
        mountPoint = EnsureChild(mountPoint, "MountPoint", transform, new Vector3(0f, -1.55f, 0f));
        gaugeAnchor = EnsureChild(gaugeAnchor, "GaugeAnchor", transform, new Vector3(0f, 1.4f, 0f));
        turretRestLocalPosition = turret.localPosition;
        penetrationData = CreateWeapon(penetrationTemplate, penetrationAttack);
        explosionData = CreateWeapon(explosionTemplate, explosionAttack);
        if (generateFallbackVisuals) BuildPrototypeVisuals();
        if (generateFallbackCollider && cannonBase.GetComponentInChildren<Collider2D>() == null)
        {
            int stageLayer = LayerMask.NameToLayer("Stage");
            if (stageLayer >= 0) cannonBase.gameObject.layer = stageLayer;
            BoxCollider2D solid = cannonBase.gameObject.AddComponent<BoxCollider2D>();
            solid.size = new Vector2(2.3f, 1.8f);
        }
        BuildGauge();
        UpdateGauge();
    }

    static Transform EnsureChild(Transform value, string childName, Transform parent, Vector3 position)
    {
        if (value != null) return value;
        Transform existing = parent.Find(childName);
        if (existing != null) return existing;
        Transform child = new GameObject(childName).transform;
        child.SetParent(parent, false);
        child.localPosition = position;
        return child;
    }

    static weapondata CreateWeapon(weapondata template, float attack)
    {
        weapondata data = template != null ? Instantiate(template) : ScriptableObject.CreateInstance<weapondata>();
        data.name = "StageCannon_RuntimeWeapon";
        data.Attack = attack;
        return data;
    }

    static bool CanOperate()
    {
        SystemManager manager = SystemManager.Instance;
        return manager != null &&
            (manager.gameMode == SystemManager.GameMode.Nomal ||
             manager.gameMode == SystemManager.GameMode.Goal);
    }

    public static void InterruptActive()
    {
        if (active != null) active.Release();
    }

    void OnEnable()
    {
        GlobalEvents.OnPlayerDie += OnPlayerDie;
        GlobalEvents.OnNextSpot += OnNextSpot;
    }

    void OnDisable()
    {
        GlobalEvents.OnPlayerDie -= OnPlayerDie;
        GlobalEvents.OnNextSpot -= OnNextSpot;
        Release();
        ClearShots();
        ClearPulses();
    }

    void OnPlayerDie(float unused)
    {
        Release();
        ClearShots();
    }

    void OnNextSpot(int unused)
    {
        Release();
        ClearShots();
    }

    void OnDestroy()
    {
        if (penetrationData != null) Destroy(penetrationData);
        if (explosionData != null) Destroy(explosionData);
        if (whiteSprite != null) Destroy(whiteSprite);
        if (ringSprite != null) Destroy(ringSprite);
        if (diskSprite != null) Destroy(diskSprite);
        if (ringTexture != null) Destroy(ringTexture);
        if (diskTexture != null) Destroy(diskTexture);
    }

    bool MountHeld()
    {
        return (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) ||
            (optionalMountAction != null && optionalMountAction.action != null &&
             optionalMountAction.action.enabled && optionalMountAction.action.IsPressed());
    }

    bool FirePressed()
    {
        return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (optionalFireAction != null && optionalFireAction.action != null &&
             optionalFireAction.action.enabled && optionalFireAction.action.WasPressedThisFrame());
    }

    void Update()
    {
        reloadRemaining = Mathf.Max(0f, reloadRemaining - Time.deltaTime);
        UpdateGauge();
        TickZoom();
        TickRecoil();
        TickPulses();

        bool held = MountHeld();
        if (!held) { holdConsumed = false; holdElapsed = 0f; }

        if (!CanOperate())
        {
            if (state != State.Idle || zoomOwned) Release();
            holdElapsed = 0f;
            ClearShots();
            return;
        }

        if (state == State.Idle)
        {
            PlayerController player = PlayerController.Instance;
            if (!held || holdConsumed || !IsLoaded || active != null ||
                player == null || player.HP <= 0f || player.IsDialogueInputBlocked ||
                player.IsCannonInputBlocked || player.movetype != PlayerController.MoveType.Nomal ||
                Vector2.Distance(player.transform.position, mountPoint.position) > interactionDistance)
            {
                holdElapsed = 0f;
                return;
            }
            holdElapsed += Time.deltaTime;
            if (holdElapsed >= holdSeconds) TryMount(player);
            return; // A click on the mounting frame cannot also fire the cannon.
        }

        if (owner == null || owner.HP <= 0f)
        {
            Release();
            return;
        }
        if (state == State.Aiming)
        {
            if (!owner.IsCannonMounted) { Release(); return; }
            UpdateAim();
            if (Time.time >= nextMarkerRefresh)
            {
                RefreshMarkers();
                nextMarkerRefresh = Time.time + markerRefreshSeconds;
            }
            if (FirePressed()) Fire();
        }
        else if (state == State.AwaitingEscape)
        {
            // No cancellation by Space release, timer, or further clicks.
            if (!owner.IsCannonMounted) { Release(); return; }
            if (owner.MoveInput.sqrMagnitude > 0.01f &&
                owner.BeginStageCannonEscape(owner.MoveInput, escapeSpeedMultiplier))
            {
                state = State.Escaping;
                BeginRestore();
            }
        }
        else if (state == State.Escaping && !owner.IsCannonEscaping)
        {
            state = State.Idle;
            owner = null;
            if (active == this) active = null;
        }
    }

    void TryMount(PlayerController player)
    {
        if (!player.TryMountStageCannon(mountPoint.position)) return;
        active = this;
        owner = player;
        state = State.Aiming;
        holdConsumed = true;
        holdElapsed = 0f;
        if (owner.playerBuff != null)
            owner.playerBuff.GrantStageCannonInvincibility(invincibleSeconds);
        foreach (EnemyBase enemy in FindObjectsOfType<EnemyBase>())
            if (IsHostile(enemy)) charaDamage.ForceStageCannonConfusion(enemy);
        BeginZoom();
        UpdateAim(true);
        RefreshMarkers();
        nextMarkerRefresh = Time.time + markerRefreshSeconds;
    }

    void UpdateAim(bool snap = false)
    {
        Camera camera = zoomCamera != null ? zoomCamera :
            (aimCamera != null ? aimCamera : Camera.main);
        Vector2 world;
        if (Mouse.current != null && camera != null)
        {
            Vector2 screen = Mouse.current.position.ReadValue();
            Vector3 point = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y,
                Mathf.Abs(camera.transform.position.z - turret.position.z)));
            world = point;
        }
        else if (owner != null && owner.cursorController != null)
            world = owner.cursorController.MousePos;
        else return;
        Vector2 direction = world - (Vector2)turret.position;
        if (direction.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion desired = Quaternion.Euler(0f, 0f, angle + turretAngleOffset);
        turret.rotation = snap || aimDegreesPerSecond <= 0f ? desired :
            Quaternion.RotateTowards(turret.rotation, desired, aimDegreesPerSecond * Time.deltaTime);
        float actualAngle = (turret.eulerAngles.z - turretAngleOffset) * Mathf.Deg2Rad;
        aimDirection = new Vector2(Mathf.Cos(actualAngle), Mathf.Sin(actualAngle));
    }

    void Fire()
    {
        if (state != State.Aiming || !IsLoaded || owner == null) return;
        state = State.AwaitingEscape;
        reloadRemaining = reloadSeconds;
        UpdateGauge();
        ClearMarkers();
        penetrationData.Attack = penetrationAttack;
        explosionData.Attack = explosionAttack;
        Vector2 origin = firePoint.position;
        Quaternion rotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg);
        GameObject visual = projectileVisualPrefab != null
            ? Instantiate(projectileVisualPrefab, origin, rotation, transform)
            : MakeRect("CannonShell", transform, origin, new Vector3(0.72f, 0.25f, 1f),
                new Color(1f, 0.82f, 0.25f), feedbackSortingOrder, true).gameObject;
        visual.transform.rotation = rotation;
        shots.Add(new Shot { player = owner, position = origin, direction = aimDirection,
            remaining = projectileDistance, visual = visual });
        SpawnEffect(muzzleEffectPrefab, origin, rotation, 0.4f, 1.6f,
            new Color(1f, 0.85f, 0.25f, 0.9f), fireEffectSeconds);
        if (fireSE != null && AudioManager.instance != null) AudioManager.instance.PlaySE(fireSE);
        if (owner.cameraM != null) owner.cameraM.Shake(shakeSeconds, shakeStrength);
        recoilStart = Time.time;
        recoilLocalDirection = turret.parent != null
            ? turret.parent.InverseTransformVector(-aimDirection) : (Vector3)(-aimDirection);
    }

    void FixedUpdate()
    {
        if (!CanOperate()) return;
        for (int i = shots.Count - 1; i >= 0; i--)
        {
            Shot shot = shots[i];
            if (shot.player == null || shot.player.HP <= 0f || !StepShot(shot))
            {
                if (shot.visual != null) Destroy(shot.visual);
                shots.RemoveAt(i);
            }
        }
    }

    bool StepShot(Shot shot)
    {
        float distance = Mathf.Min(shot.remaining, projectileSpeed * Time.fixedDeltaTime);
        if (distance <= 0f) return false;
        // Sweep the path so high-speed shots cannot tunnel through enemy hitboxes.
        // Terrain is intentionally ignored so the cannon can reach vehicles outside the train.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(shot.position, projectileRadius,
            shot.direction, distance, enemyHitLayers);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit2D hit in hits)
        {
            if (!TryGetReceiver(hit.collider, out charaDamage receiver, out EnemyBase enemy) ||
                !shot.penetrated.Add(enemy)) continue;
            bool detonates = IsLargeTarget(enemy); // cache before damage can kill it
            Vector2 impact = shot.position + shot.direction * hit.distance;
            ApplyHit(receiver, enemy, penetrationData, shot.player, hit.collider.gameObject, impact);
            if (detonates)
            {
                Explode(shot, impact, receiver, enemy);
                return false;
            }
        }
        shot.position += shot.direction * distance;
        shot.remaining -= distance;
        if (shot.visual != null) shot.visual.transform.position = shot.position;
        return shot.remaining > 0.0001f;
    }

    static bool IsHostile(EnemyBase enemy)
    {
        return enemy != null && enemy.isActiveAndEnabled && enemy.HP > 0f &&
            enemy.status != EnemyBase.Status.Die && enemy.status != EnemyBase.Status.MovieStay &&
            !enemy.IsCharmed && (enemy.Factions & Faction.Charmed) == 0;
    }

    static bool IsLargeTarget(EnemyBase enemy)
    {
        if (enemy == null || enemy.charadata == null) return false;
        var strength = enemy.charadata.enemyStrength;
        return strength == charadata.EnemyStrength.Veteran ||
            strength == charadata.EnemyStrength.Elite || strength == charadata.EnemyStrength.Boss;
    }

    static bool TryGetReceiver(Collider2D hit, out charaDamage receiver, out EnemyBase enemy)
    {
        receiver = null;
        enemy = null;
        if (hit == null || !hit.gameObject.CompareTag("EnemyArea")) return false;
        receiver = hit.GetComponent<charaDamage>();
        if (receiver == null) return false;
        enemy = receiver.OwnerEnemyBase;
        return IsHostile(enemy);
    }

    void Explode(Shot shot, Vector2 position, charaDamage direct, EnemyBase directEnemy)
    {
        SpawnEffect(explosionEffectPrefab, position, Quaternion.identity, 0.5f,
            explosionRadius * 2f, new Color(1f, 0.49f, 0.12f, 0.85f), fireEffectSeconds);
        if (explosionSE != null && AudioManager.instance != null)
            AudioManager.instance.PlaySE(explosionSE);
        if (shot.player != null && shot.player.cameraM != null)
            shot.player.cameraM.Shake(shakeSeconds, shakeStrength);
        HashSet<EnemyBase> damaged = new HashSet<EnemyBase>();
        // Direct target receives its second hit even if its collider moved.
        if (direct != null && IsHostile(directEnemy))
        {
            damaged.Add(directEnemy);
            ApplyHit(direct, directEnemy, explosionData, shot.player, direct.gameObject, position);
        }
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(position, explosionRadius, enemyHitLayers))
        {
            if (!TryGetReceiver(hit, out charaDamage receiver, out EnemyBase enemy) ||
                !damaged.Add(enemy)) continue;
            ApplyHit(receiver, enemy, explosionData, shot.player, hit.gameObject, hit.ClosestPoint(position));
        }
    }

    // Same non-skill weapon formula, critical roll, event, hit effects and
    // DamageFromPlayer route as PlayerAttack. Not a card slot.
    static void ApplyHit(charaDamage receiver, EnemyBase enemy, weapondata data,
        PlayerController player, GameObject eventTarget, Vector2 position)
    {
        if (receiver == null || !IsHostile(enemy) || data == null || player == null ||
            player.playerBuff == null) return;
        float attack = PlayerAttack.CalculateWeaponDamage(data, player, 0, false);
        bool critical = player.AddCritical + player.Critical > UnityEngine.Random.Range(0f, 100f);
        if (data.HitAudio != null && AudioManager.instance != null)
            AudioManager.instance.PlaySE(data.HitAudio);
        GlobalEvents.InvokePlayerAttack(eventTarget, attack, critical ? 3f : 1f, critical);
        if (!IsHostile(enemy)) return;
        if (data.statusEffects != null)
            foreach (StatusEffectAttack effect in data.statusEffects)
                if (effect != null && IsHostile(enemy)) receiver.SpawnBuff(effect);
        if (!IsHostile(enemy)) return;
        float actual = critical ? (attack * 3f + 5f) *
            Mathf.Max(0f, 1f + player.AddLuckyDamagePercent / 100f) : attack;
        receiver.DamageFromPlayer(actual, data.StaggerPower, data.ConfusionPower,
            critical, player.AddConfusionDamagePercent, true);
        player.HitEffectSpawn(position);
    }

    void ClearShots()
    {
        foreach (Shot shot in shots)
            if (shot.visual != null) Destroy(shot.visual);
        shots.Clear();
    }

    void Release()
    {
        if (owner != null) owner.AbortStageCannonControl();
        owner = null;
        state = State.Idle;
        holdElapsed = 0f;
        ClearMarkers();
        RestoreImmediately();
        if (turret != null) turret.localPosition = turretRestLocalPosition;
        recoilStart = -100f;
        if (active == this) active = null;
    }

    void BeginZoom()
    {
        zoomCamera = aimCamera;
        if (zoomCamera == null && owner != null && owner.cameraM != null)
            zoomCamera = owner.cameraM.GetComponent<Camera>();
        if (zoomCamera == null) zoomCamera = Camera.main;
        if (zoomCamera == null || !zoomCamera.orthographic) return;
        if (!zoomOwned) normalSize = zoomCamera.orthographicSize;
        zoomOwned = true;
        StartZoomTransition(normalSize * zoomMultiplier, zoomSeconds, false);
    }

    void StartZoomTransition(float size, float duration, bool restoring)
    {
        if (!zoomOwned || zoomCamera == null) return;
        zoomStart = zoomCamera.orthographicSize;
        zoomTarget = size;
        zoomElapsed = 0f;
        zoomDuration = duration;
        zoomRestoring = restoring;
        if (duration <= 0f)
        {
            zoomCamera.orthographicSize = size;
            if (restoring) zoomOwned = false;
        }
    }

    void BeginRestore()
    {
        if (zoomOwned) StartZoomTransition(normalSize, restoreSeconds, true);
    }

    void TickZoom()
    {
        if (!zoomOwned || zoomCamera == null || zoomDuration <= 0f || zoomElapsed >= zoomDuration) return;
        zoomElapsed = Mathf.Min(zoomDuration, zoomElapsed + Time.deltaTime);
        float t = zoomElapsed / zoomDuration;
        float eased = 1f - Mathf.Pow(1f - t, 3f);
        zoomCamera.orthographicSize = Mathf.Lerp(zoomStart, zoomTarget, eased);
        if (zoomElapsed >= zoomDuration && zoomRestoring) zoomOwned = false;
    }

    void RestoreImmediately()
    {
        if (zoomOwned && zoomCamera != null) zoomCamera.orthographicSize = normalSize;
        zoomOwned = false;
        zoomCamera = null;
    }

    void TickRecoil()
    {
        if (turret == null || fireEffectSeconds <= 0f) return;
        float t = Mathf.Clamp01((Time.time - recoilStart) / fireEffectSeconds);
        turret.localPosition = turretRestLocalPosition +
            recoilLocalDirection * recoilDistance * (1f - t) * (1f - t);
    }

    void RefreshMarkers()
    {
        foreach (EnemyBase enemy in FindObjectsOfType<EnemyBase>())
        {
            if (!IsHostile(enemy) || !IsLargeTarget(enemy) || markers.ContainsKey(enemy)) continue;
            GameObject marker;
            if (targetMarkerPrefab != null)
                marker = Instantiate(targetMarkerPrefab, transform);
            else
            {
                marker = new GameObject("CannonTarget");
                marker.transform.SetParent(transform, false);
                SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
                sr.sprite = RingSprite();
                sr.color = new Color(1f, 0.35f, 0.15f, 0.95f);
                sr.sortingOrder = feedbackSortingOrder + 1;
                marker.transform.localScale = Vector3.one * 0.7f;
            }
            marker.transform.position = enemy.transform.position + (Vector3)markerOffset;
            markers.Add(enemy, marker);
        }
    }

    void LateUpdate()
    {
        if (state != State.Aiming) return;
        staleMarkers.Clear();
        foreach (var pair in markers)
        {
            if (!IsHostile(pair.Key) || !IsLargeTarget(pair.Key) || pair.Value == null)
            {
                if (pair.Value != null) Destroy(pair.Value);
                staleMarkers.Add(pair.Key);
            }
            else pair.Value.transform.position = pair.Key.transform.position + (Vector3)markerOffset;
        }
        foreach (EnemyBase enemy in staleMarkers) markers.Remove(enemy);
    }

    void ClearMarkers()
    {
        foreach (GameObject marker in markers.Values)
            if (marker != null) Destroy(marker);
        markers.Clear();
        staleMarkers.Clear();
    }

    Sprite WhiteSprite()
    {
        if (whiteSprite == null)
        {
            Texture2D tex = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), tex.width, 0, SpriteMeshType.FullRect);
        }
        return whiteSprite;
    }

    static Sprite MakeCircle(bool ring, out Texture2D tex)
    {
        const int size = 64;
        tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
            bool visible = d <= 30f && (!ring || d >= 25f ||
                (d >= 18f && (Mathf.Abs(x - 31.5f) < 1.5f || Mathf.Abs(y - 31.5f) < 1.5f)));
            pixels[y * size + x] = new Color32(255, 255, 255, visible ? (byte)255 : (byte)0);
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
    }

    Sprite RingSprite()
    {
        if (ringSprite == null) ringSprite = MakeCircle(true, out ringTexture);
        return ringSprite;
    }

    Sprite DiskSprite()
    {
        if (diskSprite == null) diskSprite = MakeCircle(false, out diskTexture);
        return diskSprite;
    }

    SpriteRenderer MakeRect(string objectName, Transform parent, Vector3 position,
        Vector3 scale, Color color, int order, bool world = false)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        if (world) go.transform.position = position;
        else go.transform.localPosition = position;
        go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite();
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    void BuildPrototypeVisuals()
    {
        if (cannonBase.GetComponentInChildren<SpriteRenderer>() == null)
        {
            MakeRect("BaseHull", cannonBase, Vector3.zero, new Vector3(2.5f, 2f, 1f),
                new Color(0.16f, 0.19f, 0.22f), cannonSortingOrder);
            MakeRect("BasePlate", cannonBase, Vector3.zero, new Vector3(2.22f, 1.72f, 1f),
                new Color(0.30f, 0.34f, 0.38f), cannonSortingOrder + 1);
        }
        if (turret.GetComponentInChildren<SpriteRenderer>() == null)
        {
            MakeRect("TurretHousing", turret, Vector3.zero, new Vector3(1.3f, 1.25f, 1f),
                new Color(0.40f, 0.44f, 0.48f), cannonSortingOrder + 2);
            MakeRect("Barrel", turret, new Vector3(1.13f, 0f, 0f), new Vector3(1.8f, 0.42f, 1f),
                new Color(0.25f, 0.28f, 0.32f), cannonSortingOrder + 3);
            MakeRect("Muzzle", turret, new Vector3(1.95f, 0f, 0f), new Vector3(0.32f, 0.62f, 1f),
                new Color(0.46f, 0.49f, 0.52f), cannonSortingOrder + 4);
        }
    }

    void BuildGauge()
    {
        MakeRect("ReloadBackground", gaugeAnchor, Vector3.zero,
            new Vector3(gaugeWidth + 0.1f, gaugeHeight + 0.1f, 1f),
            new Color(0.06f, 0.07f, 0.08f, 0.9f), feedbackSortingOrder);
        gaugeFillRenderer = MakeRect("ReloadFill", gaugeAnchor, Vector3.zero,
            new Vector3(gaugeWidth, gaugeHeight, 1f), Color.green, feedbackSortingOrder + 1);
        gaugeFill = gaugeFillRenderer.transform;
    }

    void UpdateGauge()
    {
        if (gaugeFill == null) return;
        float width = gaugeWidth * ReloadProgress;
        gaugeFill.localScale = new Vector3(width, gaugeHeight, 1f);
        gaugeFill.localPosition = new Vector3((width - gaugeWidth) * 0.5f, 0f, 0f);
        gaugeFillRenderer.color = IsLoaded ? new Color(0.34f, 0.92f, 0.48f) :
            new Color(1f, 0.65f, 0.18f);
    }

    void SpawnEffect(GameObject prefab, Vector2 position, Quaternion rotation,
        float from, float to, Color color, float duration)
    {
        if (prefab != null)
        {
            GameObject effect = Instantiate(prefab, position, rotation, transform);
            Destroy(effect, Mathf.Max(0.1f, duration));
            return;
        }
        GameObject go = new GameObject("CannonFlash");
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * from;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = DiskSprite();
        sr.color = color;
        sr.sortingOrder = feedbackSortingOrder;
        pulses.Add(new Pulse { visual = go, renderer = sr, from = Vector3.one * from,
            to = Vector3.one * to, color = color, start = Time.time, duration = Mathf.Max(0.01f, duration) });
    }

    void TickPulses()
    {
        for (int i = pulses.Count - 1; i >= 0; i--)
        {
            Pulse pulse = pulses[i];
            if (pulse.visual == null) { pulses.RemoveAt(i); continue; }
            float t = Mathf.Clamp01((Time.time - pulse.start) / pulse.duration);
            pulse.visual.transform.localScale = Vector3.Lerp(pulse.from, pulse.to,
                1f - Mathf.Pow(1f - t, 3f));
            Color color = pulse.color;
            color.a *= 1f - t;
            pulse.renderer.color = color;
            if (t >= 1f) { Destroy(pulse.visual); pulses.RemoveAt(i); }
        }
    }

    void ClearPulses()
    {
        foreach (Pulse pulse in pulses)
            if (pulse.visual != null) Destroy(pulse.visual);
        pulses.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(mountPoint != null ? mountPoint.position :
            transform.TransformPoint(new Vector3(0f, -1.55f, 0f)), interactionDistance);
        Gizmos.color = Color.yellow;
        if (firePoint != null) Gizmos.DrawWireSphere(firePoint.position, projectileRadius);
    }
}
