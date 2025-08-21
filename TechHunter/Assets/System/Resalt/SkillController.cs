using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SkillController : MonoBehaviour
{
    [SerializeField] GameObject ResultCanvas;
    [SerializeField] GameObject Page;
    [SerializeField] GameObject SkillPage;
    [SerializeField] GameObject AllSkillPage;
    public List<GameObject> AllSkillPages = new List<GameObject>();
    [SerializeField] system_GameModeManager gameModeManager;

    [SerializeField] SkillCardData skillNoneData;

    public List<Skill> SkillList = new List<Skill>();
    public List<StatusUP> StatusUPs = new List<StatusUP>();
    public List<Relic> Relics = new List<Relic>();

    [HideInInspector]public List<PageManager> SavePage = new List<PageManager>();
    [HideInInspector]public List<StatusUP> SaveStatus = new List<StatusUP>();

    [HideInInspector] public List<SkillPageManager> SavesSkillPageManager = new List<SkillPageManager>();
    [HideInInspector] public List<Skill> SaveSkills = new List<Skill>();
    [SerializeField] TextMeshProUGUI ReRollText;
    [SerializeField] Animator ReRollAnim;
    StatusUP SaveStatusUP;
    Skill SaveSkill;

    float ReRollCost = 0;
    public int ReRollCount = 0;

    AllSkilPagel allskillpage;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    public enum ChoiceCardMode 
    {
        
        Skill,
        Status,
        None
    
    }
    ChoiceCardMode cardMode = ChoiceCardMode.None;

    PlayerController playerController => PlayerController.Instance;
    SystemManager systemManager;
    // Start is called before the first frame update
    void Start()
    {
        systemManager = GetComponent<SystemManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) 
        {
            isStartPageChoice();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            isAll_StartSkillPageChoice();
        }
        if (systemManager.gameMode == SystemManager.GameMode.Result) 
        {
            ReRollCost = 10 * (ReRollCount+1) + gameModeManager.AllGameLevel * 5;
            if (ReRollCost <= 0) 
            {
                ReRollCost = 1;
            }
            ReRollText.text = ReRollCost.ToString("F0");
        }
    }
    public void ReROLL() 
    {
        if (cardMode != ChoiceCardMode.None) 
        {
            if (playerController.AllCoins > ReRollCost) {
                playerController.AllCoins -= (int)ReRollCost;
                ReRollCount++;
                if (cardMode == ChoiceCardMode.Skill)
                {
                    isStartSkillPageChoice();
                }
                else if (cardMode == ChoiceCardMode.Status)
                {
                    isStartPageChoice();
                }
            }
        }
    }
    public void isAll_StartSkillPageChoice() 
    {
        isStartSkillPageChoice();
        GameObject CL_AllSkillPage = Instantiate(AllSkillPage);
        playerController.movetype = PlayerController.MoveType.Wait;
        AllSkillPages.Add(CL_AllSkillPage);

         allskillpage = CL_AllSkillPage.GetComponent<AllSkilPagel>();
        allskillpage.skillController = this;
        CL_AllSkillPage.transform.parent = ResultCanvas.transform;

        Change_AllSkillPage();
    }
    public void Change_AllSkillPage() 
    {
        if (allskillpage != null) 
        {
            
            int i = 0;
            foreach (var ALL_p in allskillpage.AllSkillPagesList)
            {
                ALL_p.SkillImage.sprite = playerController.skillINFO[i].skillDATA.SkillImage;
                ALL_p.PageControllText.text = playerController.skillINFO[i].PC_ControllText;
                ALL_p.LevelText.text = playerController.skillINFO[i].SkillLevel.ToString();

                i++;
            }
        }
    }
    public void isStartSkillPageChoice() 
    {
        cardMode = ChoiceCardMode.Skill;
        ReRollAnim.Play("出現");

        List<Skill> NowSkill = new List<Skill>();

        for (int i = 0; i < 3; i++)
        {
            bool Set = false;
            
                List<Skill> SaveStatusUps_R1 = new List<Skill>();
                foreach (Skill s in SkillList)
                {

                    if (s.Rarity >= 2)
                    {
                        SaveStatusUps_R1.Add(s);
                    }

                }

                
            
            while (!Set)
            {
                SaveSkill = SaveStatusUps_R1[Random.Range(0, SaveStatusUps_R1.Count)];

                int j = 0;
                foreach (var a in NowSkill) 
                {
                    if (a == SaveSkill) 
                    { 
                    j++;
                    }
                }
                if (j == 0) 
                { 
                Set = true;
                }
                
            }
            NowSkill.Add(SaveSkill);
            isSkillPage(SaveSkill, i);
        }
    }
    public void isStartPageChoice() 
    {
        ReRollAnim.Play("出現");
        playerController.movetype = PlayerController.MoveType.Wait;
        cardMode = ChoiceCardMode.Status;

        List <StatusUP> NowPage = new List<StatusUP>();
        for (int i = 0; i < 3; i++)
        {
            bool Set = false;

            List<StatusUP> SaveStatusUps_R1 = new List<StatusUP>();
                foreach (StatusUP s in StatusUPs) 
                {
                    if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.NomalBattleSpot)
                    {
                        if (s.Rarity == 1)
                        {
                            SaveStatusUps_R1.Add(s);
                        }
                    }
                    else
                    {
                        SaveStatusUps_R1.Add(s);
                    }
                }

                while (!Set)
                {
                    SaveStatusUP = SaveStatusUps_R1[Random.Range(0, SaveStatusUps_R1.Count)];

                    int j = 0;
                    foreach (var a in NowPage)
                    {
                        if (a == SaveStatusUP)
                        {
                            j++;
                        }

                    }
                    if (j == 0)
                    {
                        Set = true;
                    }
                }
                NowPage.Add(SaveStatusUP);
                isStatusPage(SaveStatusUP, i);
        }
    }

    public void ChoicePage() 
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        playerController.rb.velocity = Vector2.zero;

        cardMode = ChoiceCardMode.None;
        int index = 0;
        foreach (PageManager save in SavePage) 
        {

            if (save.Select)
            {
                save.move = PageManager.Move.Wait;

                playerController.AddATK += SaveStatus[index].Attack;
                playerController.AddAtkDF += SaveStatus[index].Defense;
                

                if (playerController.charadata.MAXHP + SaveStatus[index].HP + playerController.AddHP < 0)
                {
                    playerController.AddHP =  1- playerController.charadata.MAXHP;
                }
                else 
                {
                    playerController.AddHP += SaveStatus[index].HP;
                }
                
                playerController.AddRegen += SaveStatus[index].Regen;
                playerController.isRecoveryHP(SaveStatus[index].RecoveryHP);
                playerController.AddCritical += SaveStatus[index].Critical;
                playerController.AddRange += SaveStatus[index].Range;

                Debug.Log("statusセット");
                systemManager.gameMode = SystemManager.GameMode.Goal;
                playerController.movetype = PlayerController.MoveType.Nomal;
                systemManager.mode = SystemManager.ModeType.M1;

            }
            else 
            {
                save.move = PageManager.Move.Run;
            }
            index++;
            playerController.isAddStatus();
            playerController.playerDamage.HP_SliderChange();
            Destroy(save.gameObject, 5);

            if (gameModeManager.BattlePhase == 2) 
            {
                //isStartSkillPageChoice();
                isAll_StartSkillPageChoice();
            }
        }

        
        SavePage.Clear();
        SaveStatus.Clear();
    }

    public void ChoiceSkillPage()
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        cardMode = ChoiceCardMode.None;
        playerController.rb.velocity = Vector2.zero;

        int index = 0;
        foreach (SkillPageManager save in SavesSkillPageManager)
        {

            if (save.Select)
            {
                save.move = SkillPageManager.Move.Wait;

                foreach (SkillInfo skillinfo in playerController.skillINFO) 
                {
                    if (skillinfo.skillDATA == skillNoneData) 
                    {
                        systemManager.gameMode = SystemManager.GameMode.Goal;
                        playerController.movetype = PlayerController.MoveType.Nomal;
                        systemManager.mode = SystemManager.ModeType.M1;
                        Debug.Log("skillセット");
                        skillinfo.skillDATA = SaveSkills[index].skill;
                        break;
                    }
                }

            }
            else
            {
                save.move = SkillPageManager.Move.Run;
            }
            index++;
            playerController.isAddStatus();
            playerController.playerDamage.HP_SliderChange();
            Destroy(save.gameObject, 5);
        }

        if (AllSkillPages.Count != 0)
        {
            foreach (var a in AllSkillPages)
            {
                AllSkilPagel allSkil = a.GetComponent<AllSkilPagel>();
                allSkil.move = AllSkilPagel.Move.Run;

                Destroy(a, 5);
            }
        }
        AllSkillPages.Clear();

        SaveSkills.Clear();
        SavesSkillPageManager.Clear();
    }
    void isStatusPage(StatusUP statuspage,int value) 
    { 
    
        GameObject CL_Page = Instantiate(Page);
       PageManager pageManager = CL_Page.GetComponent<PageManager>();

        SavePage.Add(pageManager);
        SaveStatus.Add(statuspage);

        if (statuspage.Rarity == 1)
        {
            Debug.Log("コモン");
            pageManager.Page_rarity.sprite = pageManager.CommonPage;
        }
        if (statuspage.Rarity == 2)
        {
            Debug.Log("アンコモン");
            pageManager.Page_rarity.sprite = pageManager.UnCommonPage;
        }
        if (statuspage.Rarity == 3)
        {
            pageManager.Page_rarity.sprite = pageManager.Rare;
        }

        pageManager.SkillController = this;

        Animator animator = CL_Page.GetComponent<Animator>();
        RectTransform rect = CL_Page.GetComponent<RectTransform>();
        CL_Page.transform.parent = ResultCanvas.transform;

        pageManager.MainName.text = "ステータスUP";
        pageManager.Name.text = statuspage.PageName;
        pageManager.MainExplanation.text = statuspage.PageName;
        pageManager.Explanation.text = statuspage.Explanation;
        pageManager.UP_Number.text = statuspage.UpExplanation;
        pageManager.MainImage.sprite = statuspage.MainImage;

        rect.transform.position = new Vector2(-1200, 400);

        //設置間隔 -500 0 500
        //500 1000 1500
        int TargetPos = 500 * value - 500;
        Debug.Log(TargetPos);
        pageManager.TargetRectPos = TargetPos;
        int NumverStringCount = 0;

        string AttackString ="";
        
        string DefenceString ="";
        
        string HP_String = "";

        string RegenString = "";

        string RecoveryHP_String = "";

        List<string> AllStatusStrings = new List<string>();
            

        if (statuspage.Attack != 0) 
        {
            //pageManager.NumberExplanation.text
            NumverStringCount++;
            float AttackCount= playerController.ATK + playerController.AddATK;
            float AttackCount2 = AttackCount + statuspage.Attack;

             

            AttackString = "通常攻撃力:"+ AttackCount + "→" + AttackCount2;
            AllStatusStrings.Add(AttackString);
        }
        if (statuspage.Defense != 0) 
        {   
            float DefenceCount = 0;

            NumverStringCount++;
            DefenceCount = playerController.ATKDF + statuspage.Defense;
            DefenceString = "防御力:"+ playerController.ATKDF + "→" + DefenceCount;
            AllStatusStrings.Add(DefenceString);
        }
        if (statuspage.HP != 0) 
        {   float HP_Count = 0;
            NumverStringCount++;
            HP_Count = playerController.charadata.MAXHP + statuspage.HP;
            HP_String = "最大HP:"+ playerController.charadata.MAXHP + "→" +HP_Count;
            AllStatusStrings.Add(HP_String);
        }
        if (statuspage.Regen != 0) 
        {
            float RegenceCount = playerController.Regen ;
            float RegenceCount2 = RegenceCount + statuspage.Regen;

            RegenString = "自然回復:" + RegenceCount + "→" + RegenceCount2;
            AllStatusStrings.Add(RegenString);
        }
        if (statuspage.RecoveryHP != 0) 
        { 
        float RecoveryHP_Count = playerController.HP;
            float RecoveryHP_Count2 = RecoveryHP_Count + statuspage.RecoveryHP;
            if (playerController.MaxHP < RecoveryHP_Count2) 
            { 
            RecoveryHP_Count2 = playerController.MaxHP;
            }

            RecoveryHP_String = "残りHP:"+RecoveryHP_Count + "→" + RecoveryHP_Count2;

            AllStatusStrings.Add(RecoveryHP_String);
        }
        if (statuspage.Critical != 0) 
        { 
        
            float Critical_Count = playerController.Critical;
            float Critical_Count2 = Critical_Count + statuspage.Critical;

            string critical = "致命率:" + Critical_Count + "→" + Critical_Count2;
            AllStatusStrings.Add(critical);
        }
        if (statuspage.Range != 0) 
        { 
        
            float Range_Count = playerController.Range;
            float Range_Count2 = Range_Count + statuspage.Range;

            string Rangestring = "射程:" +Range_Count+"→"+Range_Count2; 
            AllStatusStrings.Add(Rangestring);
        }

        int index = 0;
        string AllStatusString ="";
        foreach (var item in AllStatusStrings) 
        {

            AllStatusString += item;
            
            index++;
            if (index == 2) 
            {
                index = 0;
                AllStatusString += "\n";
            }
        }

        pageManager.NumberExplanation.text = AllStatusString;
        //pageManager.NumberExplanation.text = AttackString + DefenceString  +"\n"+  HP_String + RegenString +"\n"+ RecoveryHP_String;


    }


    void isSkillPage(Skill statuspage, int value)
    {

        GameObject CL_Page = Instantiate(SkillPage);
        SkillPageManager pageManager = CL_Page.GetComponent<SkillPageManager>();

        SavesSkillPageManager.Add(pageManager);
        SaveSkills.Add(statuspage);

        pageManager.SkillController = this;
        
        Animator animator = CL_Page.GetComponent<Animator>();
        RectTransform rect = CL_Page.GetComponent<RectTransform>();
        CL_Page.transform.parent = ResultCanvas.transform;

        pageManager.MainName.text = "スキル獲得";
        pageManager.Name.text = statuspage.skill.SkillName;
        //pageManager.MainExplanation.text = statuspage.MainImage;
        pageManager.Explanation.text = statuspage.skill.SkillMessage;
        //pageManager.UP_Number.text = statuspage.UpExplanation;
        pageManager.MainImage.sprite = statuspage.skill.SkillImage;

        rect.transform.position = new Vector2(-1200, 0);

        //設置間隔 -500 0 500
        //500 1000 1500
        int TargetPos = 450 * value -200;
        Debug.Log(TargetPos);
        pageManager.TargetRectPos = TargetPos;
        
  }



}
[System.Serializable]
public class Skill 
{
    public string SkillName;
    public int Rarity = 1;
    public SkillCardData skill;
}
[System.Serializable]
public class StatusUP
{
    public Sprite MainImage;
    public string PageName;
    string StatusName = "ステータスUP";
    
    public string UpExplanation;
    [TextArea]
    public string Explanation;
    public int Rarity = 1;
    public float Attack = 0;
    public float Defense = 0;
    public float HP = 0;
    public float Regen = 0;
    public float RecoveryHP =0;
    public float Critical = 0;
    public float Range = 0;
}
[System.Serializable]
public class Relic 
{
    public Texture2D MainImage;
     string StatusName = "遺物獲得";
    [TextArea]
    public string Explanation;
    public int Rarity = 1;
    
}
