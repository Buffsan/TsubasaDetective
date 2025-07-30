using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EnemyBase : MonoBehaviour
{
    public charadata charadata;
    public GameObject DieEffect;
    public GameObject EnemyAnimBody;
    public SpriteRenderer spriteRenderer;
    public GameObject AttackArea;
    public Vector2 AttackAreaSize;
    public GameObject AttackSpawnPoint;
    public float AttackDellTimer=0.05f;
    public GameObject Player;
    public Vector2 PlayerDirection;
    public float PlayerDistance;
    public Animator animator;
    public Animator animator2;
    public float StanTime = 0.1f;
    public float ConfusionTimer = 0.2f;
    public float StanCount = 0;
    public float LookPlayerDistance = 1;
    public Slider HP_Slider;
    public Slider Confusion_Slider;
    public charaDamage CharaDamage;
    public float DefoScale = 2.5f;

    public Color DefaltColor;
    [Space]
    public float AttackCount = 0;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public string NAME;
    [HideInInspector] public float MAXHP;
    [HideInInspector] public float HP;
    [HideInInspector] public float ConfusionHP;
    [HideInInspector] public float MAXMP;
    [HideInInspector] public float SPEED;
    [HideInInspector] public float ATK;
    [HideInInspector] public float ATKDF;
    [HideInInspector] public int GOLD;
    [HideInInspector] public int XP;
    [HideInInspector] public float ConfusionResistance;
    [HideInInspector] public float StanHP;
    [Space]
    public GameObject DamageTextCanvas;
    public TextMeshProUGUI DamageCountText;

    [SerializeField] GameObject Coin;

    public AudioManager audioManager => AudioManager.instance;
    public EnemyAttack myAttack;

    public AudioClip AttackAudio;
    public AudioClip AttackStayAudio;
    public AudioClip DieAudio;
    public AudioClip ConfusionAudio;
    public GameObject ConfusionImageText;
    EnemyBase Ebase;

    public Color damageColor = Color.red;
    public GameObject DamageBody;

    public Volume volume;
    public Bloom bloom;

    public enum ModeType 
    { 
        M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24,
    }
    public ModeType mode = ModeType.M1;

    public enum MoveType 
    {
        Stay,
        Move,
        Attack,
        AI
    }
    public MoveType moveType = MoveType.Move;

    public enum AttackType 
    {
        
        A1, A2, A3, A4, A5, A6, A7,A8, A9,A10
    
    }
    public AttackType attackType = AttackType.A1;

    public enum Status
    {
        Nomal,
        Stan,
        StanNext,
        ConfusionResistance,
        Die,
        Stay
    }
    public Status status = Status.Nomal;

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        animator = EnemyAnimBody.GetComponent<Animator>();
        
        isInputCharadata();
        Ebase = GetComponent<EnemyBase>();
        spriteRenderer = EnemyAnimBody.GetComponent<SpriteRenderer>();

        //DefoScale = transform.position.x;
    }
    
    void isInputCharadata()
    {
        NAME = charadata.NAME;
        MAXHP = charadata.MAXHP;
        HP = MAXHP;
        ConfusionHP = charadata.ConfusionResistance;
        MAXMP = charadata.MAXMP;
        SPEED = charadata.SPEED;
        ATK = charadata.ATK;
        ATKDF = charadata.ATKDF;
        GOLD = charadata.GOLD;
        XP = charadata.XP;
        StanHP = charadata.StanHP;
        ConfusionResistance = charadata.ConfusionResistance;
    }

    public void AttackAreaSpawn() 
    {
        GameObject CL_AttackArea = Instantiate(AttackArea, AttackSpawnPoint.transform.position, Quaternion.identity);
        myAttack = CL_AttackArea.GetComponent<EnemyAttack>();
        Destroy( CL_AttackArea ,AttackDellTimer);
        myAttack.enemyBase = Ebase;
    }

    public void isPlayerLookBase()
    {
        PlayerDirection = Player.transform.position - transform.position;
        PlayerDistance = Vector2.Distance(Player.transform.position, transform.position);
        if (Player.transform.position.x < transform.position.x)
        {
            EnemyAnimBody.transform.localScale = new Vector2(-DefoScale, DefoScale);
        }
        else
        {
            EnemyAnimBody.transform.localScale = new Vector2(DefoScale, DefoScale);
        }
    }

    public void TakeDamageColor() 
    {
        StartCoroutine(ChangeColor());
    }
    private IEnumerator ChangeColor() 
    {
        //Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;

        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = DefaltColor;
    }

}
