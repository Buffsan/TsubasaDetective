using System.Collections;
using System.Collections.Generic;
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
    Rigidbody2D rb;
    SpriteRenderer spriterenderer;
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
        rb = GetComponent<Rigidbody2D>();
        spriterenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Also support coins placed directly in a scene, outside the pool.
        if (!initialized) InitializeCoin(coinType);
    }

    bool initialized;
    public void InitializeCoin(CoinType type)
    {
        coinType = type;
        initialized = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriterenderer == null) spriterenderer = GetComponent<SpriteRenderer>();
        if (Player == null) Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null && playercontroller == null)
            playercontroller = Player.GetComponent<PlayerController>();

        FlyCount = 0f;
        moveType = MoveType.Fly;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (type == CoinType.CopperCoin)
        {
            NowValue = CopperValue;
            spriterenderer.sprite = CopperCoin;
        }
        else if (type == CoinType.SilverCoin)
        {
            NowValue = SilverValue;
            spriterenderer.sprite = SilverCoin;
        }
        else if (type == CoinType.GoaldCoin)
        {
            NowValue = GoaldValue;
            spriterenderer.sprite = GoaldCoin;
        }
        else
        {
            NowValue = CrystalValue;
            spriterenderer.sprite = CrystalCoin;
        }

        Vector2 random = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        rb.AddForce(random.normalized * Random.Range(80f, 150f));
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (moveType == MoveType.Fly)
        {
            if (FlyCount < 0.3)
            {
                FlyCount += Time.deltaTime;
            }
            else 
            { 
            moveType = MoveType.Stay;
                rb.velocity = new Vector2(rb.velocity.x*0.1f, rb.velocity.y*0.1f);
            }

        }
        if (moveType == MoveType.Stay)
        {
            float PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
            if (PlayerDistance < 1.5)
            {
                moveType = MoveType.Chase;
            }
        } else if (moveType == MoveType.Chase) 
        { 
        Vector2 PlayerDistance = Player.transform.position - transform.position;
            rb.velocity = PlayerDistance.normalized * 8;
            //rb.AddForce(PlayerDistance.normalized *10);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArea") && playercontroller != null) 
        {
            playercontroller.AllCoins += NowValue;
            audiomanager.PlaySE(CoinGet);
            if (SystemManager.Instance != null)
                SystemManager.Instance.coinManagers.Remove(this);
            CoinPool.Instance.Release(this.gameObject);
            //Destroy(gameObject);
        }
    }
}
