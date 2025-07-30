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
    MoveType moveType = MoveType.Fly;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        playercontroller = Player.GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        spriterenderer = GetComponent<SpriteRenderer>();

        Vector2 radom = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        //rb.velocity = radom.normalized *Random.Range(1f,2f);
        rb.AddForce(radom.normalized * Random.Range(80f, 150f));

        if (coinType == CoinType.CopperCoin)
        {
            NowValue = CopperValue;
            spriterenderer.sprite =CopperCoin;
        }
        else if (coinType == CoinType.SilverCoin)
        {
            NowValue = SilverValue;
            spriterenderer.sprite = SilverCoin;
        }
        else if (coinType == CoinType.GoaldCoin) 
        {
            NowValue = GoaldValue;
            spriterenderer.sprite = GoaldCoin;
        }
        else if (coinType == CoinType.CrystalCoin)
        {
            NowValue = CrystalValue;
            spriterenderer.sprite = CrystalCoin;
        }
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
        if (other.CompareTag("PlayerArea")) 
        {
            playercontroller.AllCoins += NowValue;
            audiomanager.isPlaySE(CoinGet);
            Destroy(gameObject);
        }
    }
}
