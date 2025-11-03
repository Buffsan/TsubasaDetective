using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PlayerController : PlayerStatus
{
    public static PlayerController Instance;
    public Camera_Controller cameraM;
    public charadata charadata;
    public PlayerSkillBase SaveSkillBase;
    public PlayerDamage playerDamage;
    public CursorController cursorController;
    public Vector2 CursorDirection;
    public PlayerBuffController playerBuff;
    public Slider HP_Slider;
    public RectTransform HP_RectTransform;
    public TextMeshProUGUI HP_Text;
    public bool Dash = false;
    [SerializeField] TextMeshProUGUI CoinText;
    [SerializeField] GameObject Weapon;
    [Space]
    [SerializeField] GameObject Skill1;
    [SerializeField] GameObject Skill2;
    [SerializeField] GameObject Skill3;
    [SerializeField] GameObject Skill4;
    [SerializeField] GameObject Skill5;
    [SerializeField] GameObject Skill6;
    [SerializeField] GameObject knife;
    [Space]
    [SerializeField] BuffData DogeBuff;
    public GameObject SpecialBuff;
    public List<SkillInfo> skillINFO = new List<SkillInfo>();
    public List<RelicData> relicDatas = new List<RelicData>();
    public List<SkillCardData> NomalSkillData = new List<SkillCardData>();
    public RelicData NoneRelic;
    public GameObject RelicControllerObject;
    public GameObject RelicManagerObject;
    [SerializeField] GameObject HitEffect;
    public GameObject AnimatorBody;
    public GameObject MainAnimBody;
    public AudioManager audioManager => AudioManager.instance;
    int X_Scale = 1;
    [SerializeField] AudioClip Clip;
    [SerializeField] AudioClip guardClip;
    [SerializeField] AudioClip DodgeClip;
    public Rigidbody2D rb;
    public Animator animator;
    public int AllCoins=0;
    float AttackWaitCount = 0;
    float DodgeCount = 0;
    
    float AttackMoveCount = 0;

    float DontMove = 99;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    public Vector2 MoveInput = Vector2.zero;
    public Vector2 MoveInputSave = Vector2.zero;
    public Vector2 LastMoveInput = Vector2.zero;
    public enum ModeType
    {
        M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24,
    }
    public ModeType mode = ModeType.M1;

    public enum MoveType
    {
        Guard,
        Attack,
        Nomal,
        Damage,
        Dodge,
        Skill,
        NoAction,
        Wait
    }
    public MoveType movetype = MoveType.Nomal;

    public enum SkillYype 
    { 
    Skill1,
    Skill2,
    Skill3,
    Skill4,
        Skill5,
        Skill6,
        SkillStart
    }
    SkillYype skilltype = SkillYype.Skill1;

    string NAME;
    [HideInInspector] public float HP;
    [HideInInspector] public float MaxHP;
    [HideInInspector] public float MP;
    [HideInInspector] public float SPEED;
    [HideInInspector] public float ATK;
    [HideInInspector] public float ATKDF;
    [HideInInspector] public int GOLD;
    [HideInInspector] public int XP;
    [HideInInspector] public float Regen;
    [HideInInspector] public float RecoveryHP;


    

    public void OnMove(InputAction.CallbackContext context)
    {
        //Debug.Log("à⁄ìÆÇµÇΩ");
        MoveInput = context.ReadValue<Vector2>();

        
        if (MoveInput != Vector2.zero) 
        { 
        LastMoveInput = MoveInput;
        }

        if (movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            MoveInputSave = MoveInput;
            
        }
        

    }
    public void OnGuard(InputAction.CallbackContext context) 
    {
        
    }
    public void OnNomalAttack(InputAction.CallbackContext context)
    {
        //Debug.Log("çUåÇÇµÇΩ");
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            //Debug.Log("ÉXÉLÉã5");
            if (skillINFO[4].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill5;
                Skill1 = skillINFO[4].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill1.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill1);
                skillINFO[4].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }
        }
        
    }
    public void isCursorDirection() 
    {
        CursorDirection = cursorController.MousePos - transform.position;
    }
    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            //Debug.Log("ÉXÉLÉã1");
            if (skillINFO[0].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill1;
                Skill1 = skillINFO[0].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill1.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill1);
                skillINFO[0].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }
        }
    }
            public void OnSkill2(InputAction.CallbackContext context) 
    {
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            //Debug.Log("ÉXÉLÉã2");
            if (skillINFO[1].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill2;
                Skill2 = skillINFO[1].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill2.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill2);
                skillINFO[1].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }
        }
    }
    public void OnSkill3(InputAction.CallbackContext context)
    {
        //Debug.Log("ÉXÉLÉã3");
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            if (skillINFO[2].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill3;
                Skill3 = skillINFO[2].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill3.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill3);
                skillINFO[2].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }
        }
    }
    public void OnSkill4(InputAction.CallbackContext context) 
    {//Debug.Log("ÉXÉLÉãÇS");
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            if (skillINFO[3].SkillCardManager.mode == SkillCardManager.Mode.Standby) { 
            mode = ModeType.M1;
            movetype = MoveType.Skill;
            skilltype = SkillYype.Skill4;
            AttackMoveCount = 0;
            Skill4 = skillINFO[3].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill4.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill4);
                skillINFO[3].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }

        }
    }
    public void OnSkill5(InputAction.CallbackContext context)
    {
        //Debug.Log("ÉXÉLÉãÇS");
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            if (skillINFO[4].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill5;
                AttackMoveCount = 0;
                Skill5 = skillINFO[4].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill5.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill5);
                skillINFO[4].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }

        }
    }
    public void OnSkill6(InputAction.CallbackContext context)
    {
        //Debug.Log("ÉXÉLÉã6");
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait)
        {
            if (skillINFO[6].SkillCardManager.mode == SkillCardManager.Mode.Standby)
            {
                mode = ModeType.M1;
                movetype = MoveType.Skill;
                skilltype = SkillYype.Skill6;
                AttackMoveCount = 0;
                Skill6 = skillINFO[5].skillDATA.SkillData;
                PlayerSkillBase playerSkillBase = Skill6.GetComponent<PlayerSkillBase>();
                GlobalEvents.InvokeSkillUse(playerSkillBase, Skill6);
                skillINFO[5].SkillCardManager.mode = SkillCardManager.Mode.CoolTime;
            }

        }
    }



    public void OnDodge(InputAction.CallbackContext context) 
    {
        if (context.phase == InputActionPhase.Performed && movetype != MoveType.Dodge && movetype != MoveType.NoAction && movetype != MoveType.Wait) 
        {
            movetype = MoveType.Dodge;
            //playerDamage.SetInvincible(0.3f);
            playerBuff.SpawnBuff(DogeBuff);
            animator.SetBool("Dodge", true);
            audioManager.isPlaySE(DodgeClip);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        animator = AnimatorBody.GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isInputCharadata();

        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void FixedUpdate()
    {
        isCursorDirection();


        isChangeAnim();
        AttackWaitCount += Time.deltaTime;
        CoinText.text = "x" + AllCoins.ToString();
        HP_Text.text = HP.ToString() + "/" + MaxHP.ToString();
        switch (movetype) 
        { 
        
            case MoveType.Nomal:
                isMove(1);
                //animator.SetBool("Dash", false);
                animator.SetInteger("Attack", 0);
                break; 
            case MoveType.Guard:
                isGuard();
                break;
            case MoveType.Attack: 
                isAttack();
                break;
            case MoveType.Damage:

                break;
            case MoveType.Skill:
                isSkill();
                break;
            case MoveType.Dodge:
                isDodge();
                break;
            case MoveType.NoAction:
                isNoAction();
                break;
            case MoveType.Wait:
                
                break;
            

        
        }

        
    }
    public void SpawnRelic(RelicData relicData) 
    {
        int index = 0;
        foreach (var relic in relicDatas) 
        { 
        
            if(relic != NoneRelic) index++;
        
        }
        if (index >= 5) return;
        int j = 0;
        foreach (var relic in relicDatas)
        {

            if (relic == NoneRelic) break;
            j++;
        }
        relicDatas[j] = relicData;
        GameObject CL_Relic = Instantiate(RelicControllerObject, transform.position, Quaternion.identity);
        CL_Relic.transform.parent = RelicManagerObject.transform;
        RelicController relicController = CL_Relic.GetComponent<RelicController>();
        relicController.relicData = relicData;

    
    }
    void isNoAction() 
    { 
        isMove(1);
        //Debug.Log("NoAction");
    }
    void isDodge() 
    {
        DodgeCount += Time.deltaTime;
        if (DodgeCount < 0.15)
        {
            rb.velocity = rb.velocity = MoveInputSave.normalized * 10;
        }
        else if (DodgeCount < 0.35) 
        {
            rb.velocity = rb.velocity = MoveInputSave.normalized * 1.2f;
        }
        else
        { 
        DodgeCount = 0;
            rb.velocity = Vector2.zero;
            movetype = MoveType.Nomal;
            animator.SetBool("Dodge", false);
            Dash = true;
            animator.SetInteger("Anim", 0);
            MoveInputSave = MoveInput;


        }
        if (SaveSkillBase) SaveSkillBase = null;
    }
    void isDamage() 
    {
        isMove(1f);
        
    }
    void isGuard() 
    { 
    isMove(0.1f);
        animator.SetBool("Guard", true);
    }
    void isSkill() 
    {
        
        if (mode == ModeType.M1)
        {Dash = false;
            if (animator.GetInteger("Attack") == 1)
            {
                animator.SetInteger("Attack", 2);
                animator.Play("çUåÇÇQ", 0, 0);
            }
            else 
            { 
                animator.SetInteger("Attack", 1);
                animator.Play("çUåÇÇP",0,0);
            }
            
            if (skilltype == SkillYype.Skill1)
            {
                SaveSkill(Skill1,0);
            }
            else if (skilltype == SkillYype.Skill2)
            {
                SaveSkill(Skill2,1);
            }
            else if (skilltype == SkillYype.Skill3)
            {
                SaveSkill(Skill3,2);
            }
            else if (skilltype == SkillYype.Skill4)
            {
                SaveSkill(Skill4,3);
            }
            else if (skilltype == SkillYype.Skill5)
            {
                SaveSkill(Skill5,4);
            }
            else if (skilltype == SkillYype.Skill6)
            {
                SaveSkill(Skill6,5);
            }
            mode = ModeType.M2;
        }
        else if (mode == ModeType.M2) 
        {
            SaveSkillBase.SkillPlay();
        }

        /*
        isMove(0.2f);
        if (AttackMoveCount > 0.6)
        {
            AttackMoveCount = 0;
            movetype = MoveType.Nomal;
        }
        else 
        {
            AttackMoveCount += Time.deltaTime;
        }
        */
    }
    void SaveSkill(GameObject SkillOBJ,int Level) 
    {
        GameObject CL_SkillOBJ = Instantiate(SkillOBJ,transform.position,Quaternion.identity);
        CL_SkillOBJ.transform.parent = transform;

        PlayerSkillBase CL_playerSkill = CL_SkillOBJ.GetComponent<PlayerSkillBase>();
        SaveSkillBase = CL_playerSkill;

        int i = 0;
        int SkillLevel = 0;
        foreach (var skillinfo in skillINFO) 
        {
            if (i == Level) 
            {
                SkillLevel = skillinfo.SkillLevel;
            }
            i++;
        }

        CL_playerSkill.SkillLevel = SkillLevel;
    }
    void isAttack() 
    { 
        AttackWaitCount = 0;
        isMove(0.5f);

        if (mode == ModeType.M1)
        {
            animator.SetInteger("Attack", 1);
            //animator.Play("çUåÇÇP", 0, 0);
        }
        else if (mode == ModeType.M2)
        {
            animator.SetInteger("Attack", 2);
            //animator.Play("çUåÇÇQ", 0, 0);
        }

        if (AttackMoveCount > 0.2)
        {
            movetype = MoveType.Nomal;
            AttackMoveCount = 0;
            animator.SetInteger("Attack", 0);
            if (mode == ModeType.M1)
            {
                mode = ModeType.M2;
            }else if (mode == ModeType.M2) 
            {
                mode = ModeType.M1;
            }
        }
        else 
        { 
        AttackMoveCount += Time.deltaTime;
        }

        
        //movetype = MoveType.Nomal;
    }
    public void isInputCharadata() 
    {
     NAME = charadata.NAME;
     MaxHP = charadata.MAXHP + AddHP;
     HP = charadata.MAXHP + AddHP;

     MP = charadata.MAXMP;
     SPEED = charadata.SPEED * ((100 + AddSpeed) / 100);
     ATK = charadata.ATK;
     ATKDF = charadata.ATKDF + AddAtkDF;
     GOLD = charadata.GOLD;
     XP = charadata.XP;
        Regen = charadata.Regen + AddRegen;

        Critical = charadata.Critical + AddCritical;
        Range = charadata.Range + AddRange;

        playerDamage.HP_SliderChange();
        HP_RectTransform.sizeDelta = new Vector2(50 + (MaxHP * 0.3f), HP_RectTransform.sizeDelta.y);
        //RecoveryHP = AddRecoveryHP;
    }

    public void RandomLevelUp(int Value) 
    {
        
        List<SkillInfo> skillInfos = new List<SkillInfo>();
        foreach (SkillInfo info in skillINFO) 
        {
            if (info.skillDATA != ALL_System.skillController.NoneSkill) 
            { 
            skillInfos.Add(info);
            }
        }

        if (skillInfos.Count == 0)
        {
            Debug.LogWarning("ÉåÉxÉãÉAÉbÉvâ¬î\Ç»ÉXÉLÉãÇ™Ç†ÇËÇ‹ÇπÇÒÅB");
            return;
        }

        int randomSkillInfo = Random.Range(0, skillInfos.Count);
        skillInfos[randomSkillInfo].SkillLevel+= Value;
    
    }

    public void isAddStatus()
    {
        NAME = charadata.NAME;
        MaxHP = charadata.MAXHP + AddHP;

        MP = charadata.MAXMP;
        SPEED = charadata.SPEED * ((100 + AddSpeed)/100);
        ATK = charadata.ATK;
        ATKDF = charadata.ATKDF + AddAtkDF;
        Regen = charadata.Regen + AddRegen;

        Critical = charadata.Critical + AddCritical;
        Range = charadata.Range + AddRange;

        playerDamage.HP_SliderChange();

        HP_RectTransform.sizeDelta = new Vector2(80+ (MaxHP*1.5f),HP_RectTransform.sizeDelta.y);
    }
    public void isSpecialMove(float value,Vector2 moveSPinput) 
    {
        rb.velocity = moveSPinput.normalized * (SPEED + playerBuff.Speed_AllBuff) * (value * playerBuff.MultiplySpeed_AllBuff);
        DontMove = 0;
    }
    public void isMove(float value) 
    {

        if (DontMove > 0.05)
        {
            if (Dash)
            {
                animator.SetBool("Dash", true);
                rb.velocity = MoveInput.normalized * (SPEED * 1.5f + playerBuff.Speed_AllBuff) * (value * playerBuff.MultiplySpeed_AllBuff);
                if (MoveInput == Vector2.zero) { Dash = false; }

            }
            else
            {
                animator.SetBool("Dash", false);
                rb.velocity = MoveInput.normalized * (SPEED + playerBuff.Speed_AllBuff) * (value * playerBuff.MultiplySpeed_AllBuff);
            }
        }
        else 
        {
            DontMove += Time.fixedDeltaTime;
        }
    }
    void isChangeAnim() 
    {
        if (movetype == MoveType.Nomal || movetype == MoveType.Skill || movetype == MoveType.NoAction)
        {
            if (MoveInput != Vector2.zero)
            {
                animator.SetInteger("Anim", 1);
                if (MoveInput.x > 0)
                {
                    MainAnimBody.transform.localScale = new Vector2(1f, 1f);
                    X_Scale = 1;
                }
                else if (MoveInput.x < 0)
                {
                    MainAnimBody.transform.localScale = new Vector2(-1f, 1f);
                    X_Scale = -1;
                }
            }
            else
            {
                animator.SetInteger("Anim", 0);
            }
        }
    }
    public void HitEffectSpawn(Vector2 Pos) 
    {
        GameObject CL_Effect = Instantiate(HitEffect, Pos, Quaternion.identity);
        Destroy(CL_Effect ,1f);
    }
    public void isStartPointMove( GameObject StartPoint) 
    { 
    
        transform.position = StartPoint.transform.position;

    }
    public void isRecoveryHP(float HPvalue) 
    {
        HP += HPvalue;
        if (HP > MaxHP) 
        { 
        HP = MaxHP;
        }
        if (HP < 1)
        {
            HP = 1;
        }
        playerDamage.HP_SliderChange();
    }
}

[System.Serializable]
public class SkillInfo
{
    public SkillCardData skillDATA;
    public SkillCardManager SkillCardManager;

    public int SkillLevel = 1;

    public string PC_ControllText;
    public string Xbox_ControllText;


}
