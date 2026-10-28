using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] Sprite CopperCoin;
    [SerializeField] Sprite SilverCoin;
    [SerializeField] Sprite GoaldCoin;
    [SerializeField] Sprite CrystalCoin;

    public int CopperValue = 1;
    public int SilverValue = 10;
    public int GoaldValue = 100;
    public int CrystalValue = 1000;

    float FlyCount = 0;

    int NowValue = 0;

    [SerializeField] GameObject Player;
    PlayerController playercontroller;
    SpriteRenderer spriterenderer;

    // Simulates the previous 2D body's mass=1, gravity=0, linear drag=0.9.
    // FixedUpdate positioning keeps the old fixed-step visual cadence.
    [SerializeField, Min(0f)] float visualDrag = 0.9f;
    // Previous trigger circle: radius 0.1 * prefab scale 2.5 = 0.25 world units.
    [SerializeField, Min(0f)] float pickupRadius = 0.25f;
    Vector2 simulatedVelocity;

    // All coins use the same PlayerArea, so look it up only once.
    static Collider2D playerPickupArea;
    static int lastPickupAreaLookupFrame = -1;
    [SerializeField] AudioClip CoinGet;
    public enum CoinType 
    {
        CopperCoin,
        SilverCoin, 
        GoaldCoin,
            CrystalCoin
    }
    public CoinType coinType = CoinType.CopperCoin;
    AudioManager audiomanager => AudioManager.instance;
    public enum MoveType 
    { 
    
        Fly,
        Stay,
        Chase

    }
    public MoveType moveType = MoveType.Fly;

    void Awake()
    {
        spriterenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Coins placed directly in a scene still need their normal launch.
        if (!initialized)
        {
            InitializeCoin(coinType);
            Launch();
        }
    }

    bool initialized;
    public void InitializeCoin(CoinType type)
    {
        int denomination = type == CoinType.CopperCoin ? CopperValue :
            type == CoinType.SilverCoin ? SilverValue :
            type == CoinType.GoaldCoin ? GoaldValue : CrystalValue;
        InitializeCoin(type, denomination);
    }

    // actualValue can represent a stack of several coins in one physical object.
    public void InitializeCoin(CoinType type, int actualValue)
    {
        coinType = type;
        initialized = true;
        if (spriterenderer == null) spriterenderer = GetComponent<SpriteRenderer>();
        if (playercontroller == null) playercontroller = PlayerController.Instance;
        if (Player == null && playercontroller != null) Player = playercontroller.gameObject;

        FlyCount = 0f;
        moveType = MoveType.Fly;
        simulatedVelocity = Vector2.zero;
        NowValue = actualValue;

        if (type == CoinType.CopperCoin) spriterenderer.sprite = CopperCoin;
        else if (type == CoinType.SilverCoin) spriterenderer.sprite = SilverCoin;
        else if (type == CoinType.GoaldCoin) spriterenderer.sprite = GoaldCoin;
        else spriterenderer.sprite = CrystalCoin;
    }

    public void Launch()
    {
        Vector2 random = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        // Rigidbody2D.AddForce used ForceMode2D.Force (not Impulse).
        // With mass 1, its initial velocity contribution is force * fixedDeltaTime.
        simulatedVelocity = random.normalized * Random.Range(80f, 150f) * Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        if (Player == null)
        {
            if (playercontroller == null) playercontroller = PlayerController.Instance;
            if (playercontroller != null) Player = playercontroller.gameObject;
            if (Player == null) return;
        }

        float dt = Time.fixedDeltaTime;
        if (moveType == MoveType.Fly)
        {
            if (FlyCount < 0.3f)
            {
                FlyCount += dt;
            }
            else
            {
                moveType = MoveType.Stay;
                simulatedVelocity *= 0.1f;
            }
        }

        if (moveType == MoveType.Stay)
        {
            Vector2 toPlayer = (Vector2)Player.transform.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude < 1.5f * 1.5f)
                moveType = MoveType.Chase;
        }
        else if (moveType == MoveType.Chase)
        {
            Vector2 toPlayer = (Vector2)Player.transform.position - (Vector2)transform.position;
            simulatedVelocity = toPlayer.sqrMagnitude > 0f ? toPlayer.normalized * 8f : Vector2.zero;
        }

        // Box2D linear damping integration, matching the old 0.9 drag at fixed steps.
        simulatedVelocity /= 1f + visualDrag * dt;
        transform.position += (Vector3)(simulatedVelocity * dt);

        TryCollectCoin();
    }

    static Collider2D GetPlayerPickupArea()
    {
        if (playerPickupArea != null) return playerPickupArea;
        // Avoid repeating tag searches for each pooled coin on the same frame.
        if (lastPickupAreaLookupFrame == Time.frameCount) return null;
        lastPickupAreaLookupFrame = Time.frameCount;
        GameObject area = GameObject.FindGameObjectWithTag("PlayerArea");
        if (area != null) playerPickupArea = area.GetComponent<Collider2D>();
        return playerPickupArea;
    }

    void TryCollectCoin()
    {
        if (playercontroller == null) return;

        Collider2D area = GetPlayerPickupArea();
        if (area == null || !area.enabled || !area.gameObject.activeInHierarchy) return;

        Vector2 coinPosition = transform.position;
        float radiusSquared = pickupRadius * pickupRadius;

        // Cheap broad-phase check before the precise shape query.
        Bounds bounds = area.bounds;
        float dx = Mathf.Max(bounds.min.x - coinPosition.x, coinPosition.x - bounds.max.x, 0f);
        float dy = Mathf.Max(bounds.min.y - coinPosition.y, coinPosition.y - bounds.max.y, 0f);
        if (dx * dx + dy * dy > radiusSquared) return;

        // Same effective overlap as the former radius-0.1, scale-2.5 trigger circle.
        Vector2 closestPoint = area.ClosestPoint(coinPosition);
        if ((closestPoint - coinPosition).sqrMagnitude > radiusSquared) return;

        playercontroller.AllCoins += NowValue;
        if (audiomanager != null) audiomanager.PlaySE(CoinGet);
        if (SystemManager.Instance != null)
            SystemManager.Instance.coinManagers.Remove(this);
        if (CoinPool.Instance != null)
            CoinPool.Instance.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}
