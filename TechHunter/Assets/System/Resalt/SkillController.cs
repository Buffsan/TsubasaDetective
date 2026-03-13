using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    [SerializeField] GameObject ResultCanvas;
    [SerializeField] GameObject StatusMenuObject;
    [SerializeField] GameObject Page;
    [SerializeField] GameObject SkillPage;
    [SerializeField] float SkillPagesDistanse;
    [SerializeField] float SkillPageStartPos;
    [SerializeField] GameObject RelicPage;
    [SerializeField] GameObject StagePage;
    [SerializeField] GameObject AllSkillPage;
    [SerializeField] GameObject AllRelicPage;
    [SerializeField] GameObject SkillChoiseEffect;
    public List<GameObject> AllSkillPages = new List<GameObject>();
    [SerializeField] system_GameModeManager gameModeManager;

    [SerializeField] SkillCardData skillNoneData;

    public SkillCardData NoneSkill;

    public List<Skill> SkillList = new List<Skill>();
    public List<StatusUP> StatusUPs = new List<StatusUP>();
    public List<Relic> Relics = new List<Relic>();
    public List<StagePageData> Stages = new List<StagePageData>();

    [HideInInspector] public List<PageManager> SavePage = new List<PageManager>();
    public List<StatusUP> SaveStatus = new List<StatusUP>();

    [HideInInspector] public List<RelicPageManager> Save_RelicPage = new List<RelicPageManager>();
    public List<Relic> SaveRelics = new List<Relic>();

    [HideInInspector] public List<SkillPageManager> SavesSkillPageManager = new List<SkillPageManager>();
    public SkillPageManager ChangeSkillPageManger_save;
     public List<Skill> SaveSkills = new List<Skill>();

    [HideInInspector] public List<StagePageManager> SaveStageManager = new List<StagePageManager>();
    public List<StagePageData> SaveStagePages = new List<StagePageData>();
    ALL_SystemManager allSystemManager => ALL_SystemManager.Instance;

    [SerializeField] TextMeshProUGUI ReRollText;
    [SerializeField] Animator ReRollAnim;
    [SerializeField] Animator SkipAnim;
    StatusUP SaveStatusUP;
    Skill SaveSkill;
    Relic SaveRelic;
    StagePageData SaveStage;

    public int Max_skillCount = 0;
    float ReRollCost = 0;
    public int ReRollCount = 0;

    AllSkilPagel allskillpage;
    AllSkilPagel allsRelicpage;
    StatusMenu statusMenu;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    public enum ChoiceCardMode
    {

        Skill,
        Status,
        Relic,
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
        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.M) && ALL_System.systemManager.gameSystemMode == SystemManager.GameSystemMODE.Debug)
            {
                isStartPageChoice();
            }
            if (Input.GetKeyDown(KeyCode.N) && ALL_System.systemManager.gameSystemMode == SystemManager.GameSystemMODE.Debug)
            {
                isAll_StartSkillPageChoice(2);
            }
            if (Input.GetKeyDown(KeyCode.B) && ALL_System.systemManager.gameSystemMode == SystemManager.GameSystemMODE.Debug)
            {
                All_StartRelicPageSpawn();
            }
            if (Input.GetKeyDown(KeyCode.V) && ALL_System.systemManager.gameSystemMode == SystemManager.GameSystemMODE.Debug)
            {
                isStartStagePageChoice();
            }
        }
        if (systemManager.gameMode == SystemManager.GameMode.Result)
        {
            ReRollCost = 10 * (ReRollCount + 1) + gameModeManager.AllGameLevel * 5;
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
            if (playerController.AllCoins >= ReRollCost) {
                playerController.AllCoins -= (int)ReRollCost;
                ReRollCount++;


                
                if (cardMode == ChoiceCardMode.Skill)
                {
                    isStartSkillPageChoice(2);
                }
                else if (cardMode == ChoiceCardMode.Status)
                {
                    isStartPageChoice();
                }
                else if (cardMode == ChoiceCardMode.Relic) 
                {
                    isStart_RelicPageChoice();
                }
            }
        }
    }
    public void All_StartRelicPageSpawn() 
    {
        isStart_RelicPageChoice();

        ReRollAnim.Play("出現", 0, 0);
        SkipAnim.Play("出現", 0, 0);
        GameObject CL_AllSkillPage = Instantiate(AllRelicPage);
        playerController.movetype = PlayerController.MoveType.Wait;
        AllSkillPages.Add(CL_AllSkillPage);

        allsRelicpage = CL_AllSkillPage.GetComponent<AllSkilPagel>();
        allsRelicpage.skillController = this;
        CL_AllSkillPage.transform.parent = ResultCanvas.transform;

        Change_AllRelicPage();
    }
    public void isAll_StartSkillPageChoice(int RarityValue)
    {
        isStartSkillPageChoice(RarityValue);
        Max_skillCount = 0;
        foreach (SkillInfo skillInfo in playerController.skillINFO)
        {
            if (skillInfo.skillDATA != skillNoneData)
            {
                //Debug.Log("info:"+ skillInfo.skillDATA + "  noneSkill:" + NoneSkill);
                Max_skillCount++;
            }
        }
        if (gameModeManager.gameMode != system_GameModeManager.AdventureGameMode.StartSpot)
        {
            ReRollAnim.Play("出現", 0, 0);
            SkipAnim.Play("出現", 0, 0);
        }
        GameObject CL_AllSkillPage = Instantiate(AllSkillPage);
        playerController.movetype = PlayerController.MoveType.Wait;
        AllSkillPages.Add(CL_AllSkillPage);

        allskillpage = CL_AllSkillPage.GetComponent<AllSkilPagel>();
        allskillpage.skillController = this;
        CL_AllSkillPage.transform.parent = ResultCanvas.transform;

        Change_AllSkillPage();

        GameObject CL_AllRelicPage = Instantiate(AllRelicPage);
        playerController.movetype = PlayerController.MoveType.Wait;
        AllSkillPages.Add(CL_AllRelicPage);

        allsRelicpage = CL_AllRelicPage.GetComponent<AllSkilPagel>();
        allsRelicpage.skillController = this;
        CL_AllRelicPage.transform.parent = ResultCanvas.transform;

        Change_AllRelicPage();
    }
    public void Change_AllRelicPage()
    {
            if (allsRelicpage == null) return;
        

            int i = 0;
            foreach (var ALL_p in allsRelicpage.AllSkillPagesList)
            {
                if (playerController.relicDatas[i])
                {
                    
                    if(playerController.relicDatas[i].RelicImage) ALL_p.SkillImage.sprite = playerController.relicDatas[i].RelicImage;
                }
  
                i++;
            }
        
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
    public void isStartSkillPageChoice(int RarityValue)
    {
        //Debug.Log( "" + RarityValue);
        cardMode = ChoiceCardMode.Skill;
        if (gameModeManager.gameMode != system_GameModeManager.AdventureGameMode.StartSpot)
        {
            
            ReRollAnim.Play("出現");
            SkipAnim.Play("出現");
        }

        foreach (SkillPageManager save in SavesSkillPageManager)
        {
            save.move = SkillPageManager.Move.Run;
            Destroy(save.gameObject, 5);

        }
        SaveSkills.Clear();
        SavesSkillPageManager.Clear();


        List<Skill> NowSkill = new List<Skill>();

        for (int i = 0; i < 3; i++)
        {
            bool Set = false;

            List<Skill> SaveStatusUps_R1 = new List<Skill>();
            foreach (Skill s in SkillList)
            {

                if (s.Rarity == RarityValue)
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
        if (gameModeManager.gameMode != system_GameModeManager.AdventureGameMode.StartSpot)
        {
            ReRollAnim.Play("出現");
            SkipAnim.Play("出現");
        }
        playerController.movetype = PlayerController.MoveType.Wait;
        cardMode = ChoiceCardMode.Status;

        if (statusMenu == null)
        {
            GameObject CL_MenuObject = Instantiate(StatusMenuObject, new Vector3(-1000, 0, 0), Quaternion.identity);
            statusMenu = CL_MenuObject.GetComponent<StatusMenu>();
            CL_MenuObject.transform.parent = ResultCanvas.transform;

            statusMenu.SetStatus();
        }
        //もともとあったページの削除
        foreach (PageManager save in SavePage)
        {
            save.move = PageManager.Move.Run;
            Destroy(save.gameObject, 5);

        }
        SavePage.Clear();
        SaveStatus.Clear();

        List<StatusUP> NowPage = new List<StatusUP>();
        for (int i = 0; i < 3; i++)
        {
            bool Set = false;

            List<StatusUP> SaveStatusUps_R1 = new List<StatusUP>();
            foreach (StatusUP s in StatusUPs)
            {
                if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.NomalBattleSpot)
                {
                    float randomValue = Random.Range(0, 101);
                    if (randomValue < playerController.Critical)
                    {
                        if (s.Rarity == 2)
                        {
                            SaveStatusUps_R1.Add(s);
                        }
                    }
                    else
                    {
                        if (s.Rarity == 1)
                        {
                            SaveStatusUps_R1.Add(s);
                        }
                    }

                }
                else if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.MortalBattleSpot || gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.TradeSpot)
                {
                    SaveStatusUps_R1.Add(s);
                }
                else
                {
                    if (s.Rarity == 1 || s.Rarity == 2)
                    {
                        SaveStatusUps_R1.Add(s);
                    }
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

    public void isStartStagePageChoice()
    {
        if (systemManager.stageNumber >= 2) return;
        if (gameModeManager.gameMode != system_GameModeManager.AdventureGameMode.StartSpot)
        {
            ReRollAnim.Play("消える");
            SkipAnim.Play("消える");
        }
        playerController.movetype = PlayerController.MoveType.Wait;
        cardMode = ChoiceCardMode.Status;

        
        //もともとあったページの削除
        foreach (var save in SaveStageManager)
        {
            save.move = StagePageManager.Move.Run;
            Destroy(save.gameObject, 5);

        }
        SavePage.Clear();
        SaveStatus.Clear();

        List<StagePageData> NowPage = new List<StagePageData>();
        //三枚を条件に遇うように選出
        for (int i = 0; i < 3; i++)
        {
            bool Set = false;

            List<StagePageData> SaveStages_Choise = new List<StagePageData>();
            foreach (var s in Stages)
            {
                if (s.stageNumber == systemManager.stageNumber + 1) SaveStages_Choise.Add(s);
            }

            while (!Set)
            {
                SaveStage = SaveStages_Choise[Random.Range(0, SaveStages_Choise.Count)];

                int j = 0;
                foreach (var a in NowPage)
                {
                    if (a == SaveStage)
                    {
                        j++;
                    }

                }
                if (j == 0)
                {
                    Set = true;
                }
            }
            NowPage.Add(SaveStage);
            isStagePage(SaveStage, i);
        }
    }
    public void isStart_RelicPageChoice()//レリックページの抽選
    {
        ReRollAnim.Play("出現");
        SkipAnim.Play("出現");
        playerController.movetype = PlayerController.MoveType.Wait;
        cardMode = ChoiceCardMode.Relic;

       
        //もともとあったページの削除
        foreach (RelicPageManager save in Save_RelicPage)
        {
            save.move = RelicPageManager.Move.Run;
            Destroy(save.gameObject, 5);

        }
        Save_RelicPage.Clear();
        SaveRelics.Clear();

        List<Relic> NowPage = new List<Relic>();
        for (int i = 0; i < 3; i++)
        {
            bool Set = false;

            List<Relic> SaveLocal_RelicPage = new List<Relic>();
            foreach (Relic s in Relics)
            {
                SaveLocal_RelicPage.Add(s);
                
            }

            while (!Set)
            {
                SaveRelic = SaveLocal_RelicPage[Random.Range(0, SaveLocal_RelicPage.Count)];

                int j = 0;
                foreach (var a in NowPage)
                {
                    if (a == SaveRelic)
                    {
                        j++;
                    }

                }
                if (j == 0)
                {
                    Set = true;
                }
            }
            NowPage.Add(SaveRelic);
            isSet_Relic(SaveRelic, i);
        }
    }
    public void ChoicePage()
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        SkipAnim.Play("消える");
        playerController.rb.velocity = Vector2.zero;

        if (statusMenu != null)
        {
            statusMenu.move = StatusMenu.Move.Run;
            Destroy(statusMenu.gameObject, 2);
            statusMenu = null;
        }

        cardMode = ChoiceCardMode.None;
        int index = 0;
        foreach (PageManager save in SavePage)
        {

            if (save.Select)
            {
                save.move = PageManager.Move.Wait;

                playerController.AddATK += SaveStatus[index].Attack;
                playerController.AddAtkDF += SaveStatus[index].Defense;
                playerController.Add_AdditiveATK += SaveStatus[index].AdditiveATK;

                if (playerController.charadata.MAXHP + SaveStatus[index].HP + playerController.AddHP < 0)
                {
                    playerController.AddHP = 1 - playerController.charadata.MAXHP;
                }
                else
                {
                    playerController.AddHP += SaveStatus[index].HP;
                }

                playerController.AddRegen += SaveStatus[index].Regen;
                playerController.isRecoveryHP(SaveStatus[index].RecoveryHP);
                playerController.AddCritical += SaveStatus[index].Critical;
                playerController.AddRange += SaveStatus[index].Range;
                playerController.AddSpeed += SaveStatus[index].Speed;
                playerController.AddConfusionTime += SaveStatus[index].ConfusionTime;

                float coinChange = Mathf.Max((100f + SaveStatus[index].Coin) / 100f, 0f);
                // 一時的にfloatで計算してからintに戻す
                playerController.AllCoins = Mathf.RoundToInt(playerController.AllCoins * coinChange);

                if (SaveStatus[index].RandomLevelUp > 0)
                {
                    playerController.RandomLevelUp(SaveStatus[index].RandomLevelUp);
                }

                //Debug.Log("statusセット");
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
            if (ALL_System.system_GameModeManager.gameMode == system_GameModeManager.AdventureGameMode.BossBattleSpot) 
            {
                All_StartRelicPageSpawn();
            }
            if (gameModeManager.BattlePhase == 2)
            {
                //isStartSkillPageChoice();
                isAll_StartSkillPageChoice(2);
            }
        }
        SavePage.Clear();
        SaveStatus.Clear();
    }

    public void SkipPage()
    {
        systemManager.gameMode = SystemManager.GameMode.Goal;
        playerController.movetype = PlayerController.MoveType.Nomal;
        systemManager.mode = SystemManager.ModeType.M1;
        SkillChoiseEffect.SetActive(false);
        AllRunSkillPage();
        ChoicePage();
        ChoiceRelicPage();
    }
    public void AllRunSkillPage()
    {
        ChangeSkillPageManger_save = null;
        if (AllSkillPages.Count != 0)
        {
            foreach (SkillPageManager save in SavesSkillPageManager)
            {
                save.move = SkillPageManager.Move.Run;

                Destroy(save, 5);
            }
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

    public void ClickSkillFrame(int IDnumber) 
    { 
        
    }
    public void ChoiceRelicPage() 
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        SkipAnim.Play("消える");
        playerController.rb.velocity = Vector2.zero;

        if (ALL_System.system_GameStartController != null)
        {
            ALL_System.system_GameStartController.NextPhase();

        }
        

        cardMode = ChoiceCardMode.None;
        int index = 0;
        foreach (var save in Save_RelicPage)
        {

            if (save.Select)
            {
                save.move = RelicPageManager.Move.Wait;

                playerController.SpawnRelic(SaveRelics[index].data);

                //Debug.Log("statusセット");
                systemManager.gameMode = SystemManager.GameMode.Goal;
                playerController.movetype = PlayerController.MoveType.Nomal;
                systemManager.mode = SystemManager.ModeType.M1;

            }
            else
            {
                save.move = RelicPageManager.Move.Run;
            }
            index++;
            
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

        if (ALL_System.system_GameModeManager.gameMode == system_GameModeManager.AdventureGameMode.BossBattleSpot)
        {
            isStartStagePageChoice();
        }


        Save_RelicPage.Clear();
        SaveRelics.Clear();
    }
    public void ChoiceStagePage()
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        SkipAnim.Play("消える");
        playerController.rb.velocity = Vector2.zero;

        

        cardMode = ChoiceCardMode.None;
        int index = 0;
        foreach (var save in SaveStageManager)
        {

            if (save.Select)
            {
                save.move = StagePageManager.Move.Wait;

                systemManager.nextStage[systemManager.stageNumber + 1] = SaveStagePages[index].stage;

                //Debug.Log("statusセット");
                systemManager.gameMode = SystemManager.GameMode.Goal;
                playerController.movetype = PlayerController.MoveType.Nomal;
                systemManager.mode = SystemManager.ModeType.M1;

            }
            else
            {
                save.move = StagePageManager.Move.Run;
            }
            index++;

            Destroy(save.gameObject, 5);

        }

        
        SaveStageManager.Clear();
        SaveStagePages.Clear();
    }
    public void ChoiceSkillPage()//スキルページの選択
    {
        bool isMaxSkill = false;
        Max_skillCount = 0;
        foreach (SkillInfo skillInfo in playerController.skillINFO) 
        {
            if (skillInfo.skillDATA != skillNoneData) 
            {
                //Debug.Log("info:"+ skillInfo.skillDATA + "  noneSkill:" + NoneSkill);
                Max_skillCount++;
            }
        }
        
        if (Max_skillCount >= 5) 
        { 
            isMaxSkill = true;
        }
        SkillChoiseEffect.SetActive(false);
        if (isMaxSkill)
        {
            Max_ChoicePage();
        }
        else 
        { 
            Min_ChoicePage();
        }
       
    }
    void FinChoicePage() 
    {
        playerController.movetype = PlayerController.MoveType.Nomal;
        ReRollAnim.Play("消える");
        SkipAnim.Play("消える");
        cardMode = ChoiceCardMode.None;
        playerController.rb.velocity = Vector2.zero;

        ChangeSkillPageManger_save = null;
        if (ALL_System.system_GameStartController != null)
        {
            ALL_System.system_GameStartController.NextPhase();

        }
    }
    void Max_ChoicePage() 
    {
        bool LevelUP = false;
        int index = 0;
        SkillInfo LevelUP_SkillInfo;
        ChangeSkillPageManger_save = null;
        foreach (SkillPageManager save in SavesSkillPageManager) 
        {//レベルアップできるかの検証

            if (save.Select)
            {
                ChangeSkillPageManger_save = save;

                foreach (SkillInfo skillinfo in playerController.skillINFO)
                {
                    if (skillinfo.skillDATA == SaveSkills[index].skill && skillinfo.skillDATA != skillNoneData)
                    {
                        //Debug.Log("レベルアップ発見");
                        LevelUP = true;
                        LevelUP_SkillInfo = skillinfo;
                    }
                }
                if(!LevelUP)
                {
                SkillChoiseEffect.SetActive(true);
                }
            }
           
            
            
            index++;
        }
        index = 0;

        if (LevelUP) 
        {
            foreach (SkillPageManager save in SavesSkillPageManager)
            {
                save.DontTotch = true;
                if (save.Select)
                {
                    
                    save.move = SkillPageManager.Move.Wait;
                    int i = 0;
                    foreach (SkillInfo skillinfo in playerController.skillINFO)
                    {
                        if (skillinfo.skillDATA == SaveSkills[index].skill && skillinfo.skillDATA != skillNoneData)
                        {
                            skillinfo.SkillLevel++;
                            LevelUP = true;
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
                Destroy(save.gameObject, 3);
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

            FinChoicePage();
        }
        

    }
    public void ChangeSkillPage(int ID) 
    {
        SkillChoiseEffect.SetActive(false);
        int index = 0;
        foreach (SkillPageManager save in SavesSkillPageManager) 
        {
            if (save == ChangeSkillPageManger_save)
            {
                playerController.skillINFO[ID].skillDATA = SaveSkills[index].skill;
                playerController.skillINFO[ID].SkillLevel++;
                save.move = SkillPageManager.Move.Wait;
            }
            else 
            {
                save.move = SkillPageManager.Move.Run;
            }

            Destroy(save.gameObject, 5);
            index++;
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
        FinChoicePage();

       
    }
    void Min_ChoicePage() 
    {
        int index = 0;
        foreach (SkillPageManager save in SavesSkillPageManager)
        {
            save.DontTotch = true;
            if (save.Select)
            {
                save.move = SkillPageManager.Move.Wait;
                bool LevelUP = false;
                int i = 0;
                foreach (SkillInfo skillinfo in playerController.skillINFO)
                {
                    if (skillinfo.skillDATA == SaveSkills[index].skill && skillinfo.skillDATA != skillNoneData)
                    {
                        skillinfo.SkillLevel++;
                        LevelUP = true;
                    }

                }
                if (!LevelUP)
                {
                    foreach (SkillInfo skillinfo in playerController.skillINFO)
                    {
                        if (skillinfo.skillDATA == skillNoneData)
                        {
                            
                            //Debug.Log(SaveSkills[index].skill + "確認");
                            systemManager.gameMode = SystemManager.GameMode.Goal;
                            playerController.movetype = PlayerController.MoveType.Nomal;
                            systemManager.mode = SystemManager.ModeType.M1;
                            //Debug.Log("skillセット");
                            skillinfo.skillDATA = SaveSkills[index].skill;
                            break;
                        }
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
        FinChoicePage();
    }
    void isSet_Relic(Relic relic,int value) 
    {
        GameObject CL_RelicPage = Instantiate(RelicPage);
        RelicPageManager pageManager = CL_RelicPage.GetComponent<RelicPageManager>();

        Save_RelicPage.Add(pageManager);
        SaveRelics.Add(relic);

        pageManager.SkillController = this;

        Animator animator = CL_RelicPage.GetComponent<Animator>();
        RectTransform rect = CL_RelicPage.GetComponent<RectTransform>();
        CL_RelicPage.transform.parent = ResultCanvas.transform;

        pageManager.Name.text = relic.data.RelicName;
        pageManager.Explanation.text = relic.data.Explanation;
        pageManager.MainImage.sprite = relic.data.RelicImage;

        rect.transform.position = new Vector2(-1200, 1000);

        //設置間隔 -500 0 500
        //500 1000 1500
        int TargetPos = 480 * value - 350;
        //Debug.Log(TargetPos);
        pageManager.TargetRectPos = TargetPos;
    }
    void isStatusPage(StatusUP statuspage,int value) 
    { 
    //-----------------statusページ生成
        GameObject CL_Page = Instantiate(Page);
       PageManager pageManager = CL_Page.GetComponent<PageManager>();

        SavePage.Add(pageManager);
        SaveStatus.Add(statuspage);

        if (statuspage.Rarity == 1)
        {
            //Debug.Log("コモン");
            pageManager.Page_rarity.sprite = pageManager.CommonPage;
        }
        if (statuspage.Rarity == 2)
        {
            //Debug.Log("アンコモン");
            pageManager.Page_rarity.sprite = pageManager.UnCommonPage;
            pageManager.tape.sprite = pageManager.UnCommonPage_kazari;
        }
        if (statuspage.Rarity == 3)
        {
            pageManager.Page_rarity.sprite = pageManager.Rare;
            pageManager.tape.sprite = pageManager.UnCommonPage_kazari;
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
        int TargetPos = 480 * value - 700;
        //Debug.Log(TargetPos);
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
            
            AttackString = "攻撃力倍率:"+ AttackCount + "→" + AttackCount2;
            AllStatusStrings.Add(AttackString);
        }
        if (statuspage.AdditiveATK != 0)
        {
            //pageManager.NumberExplanation.text
            NumverStringCount++;
            float AttackCount = playerController.AdditiveATK + playerController.Add_AdditiveATK;
            float AttackCount2 = AttackCount + statuspage.AdditiveATK;

            AttackString = "攻撃力:" + AttackCount + "→" + AttackCount2;
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
            float NowHP = playerController.charadata.MAXHP + playerController.AddHP;
            NumverStringCount++;
            HP_Count = playerController.charadata.MAXHP+ playerController.AddHP + statuspage.HP;
            HP_String = "最大HP:"+ NowHP + "→" +HP_Count;
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

            string critical = "運:" + Critical_Count + "→" + Critical_Count2;
            AllStatusStrings.Add(critical);
        }
        if (statuspage.Range != 0) 
        { 
        
            float Range_Count = playerController.Range;
            float Range_Count2 = Range_Count + statuspage.Range;

            string Rangestring = "クールタイム速度:" +Range_Count+"→"+Range_Count2; 
            AllStatusStrings.Add(Rangestring);
        }
        if (statuspage.Speed != 0)
        {

            float Range_Count = playerController.AddSpeed;
            float Range_Count2 = playerController.AddSpeed + statuspage.Speed;

            string Rangestring = "移動速度:" + Range_Count + "→" + Range_Count2;
            AllStatusStrings.Add(Rangestring);
        }
        if (statuspage.Coin != 0)
        {

            float Range_Count = playerController.AllCoins;
            float Range_Count2 = playerController.AllCoins * ((100 + statuspage.Coin) / 100);

            string Rangestring = "所持金:" + Range_Count + "→" + Range_Count2;
            AllStatusStrings.Add(Rangestring);
        }
        if (statuspage.ConfusionTime != 0) 
        {
            float Range_Count = playerController.ConfusionTime +3;
            float Range_Count2 = playerController.ConfusionTime + statuspage.ConfusionTime + 3;

            string Rangestring = "混乱時間:" + Range_Count + "→" + Range_Count2;
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
    void isStagePage(StagePageData statuspage, int value)
    {
        //-----------------ステージページ生成
        GameObject CL_Page = Instantiate(StagePage);
        StagePageManager pageManager = CL_Page.GetComponent<StagePageManager>();

        SaveStageManager.Add(pageManager);
        SaveStagePages.Add(statuspage);


        pageManager.SkillController = this;

        Animator animator = CL_Page.GetComponent<Animator>();
        RectTransform rect = CL_Page.GetComponent<RectTransform>();
        CL_Page.transform.parent = ResultCanvas.transform;

        pageManager.MainName.text = "ステージ選択";
        pageManager.Name.text = statuspage.StageName;
        pageManager.BossIMage.sprite = statuspage.BossImage;
        pageManager.MainExplanation.text = statuspage.explaination;
        pageManager.MainImage.sprite = statuspage.StageImage;
        pageManager.BossMAXHP.text = statuspage.charadata.MAXHP.ToString();
        

        for (int i = 0; i < pageManager.StageDifficultyStar.Count; i++) 
        { 
        if(statuspage.StageDifficultyStar > i) 
            {
                pageManager.StageDifficultyStar[i].sprite = pageManager.Star;
            }
        }
        for (int i = 0; i < pageManager.BossDifficultyStar.Count; i++)
        {
            if (statuspage.BossDifficultyStar > i)
            {
                pageManager.BossDifficultyStar[i].sprite = pageManager.Star;
            }
        }

        rect.transform.position = new Vector2(-1200, 400);

        //設置間隔 -500 0 500
        //500 1000 1500
        int TargetPos = 600 * value - 700;
        //Debug.Log(TargetPos);
        pageManager.TargetRectPos = TargetPos;

        
    }

    void isSkillPage(Skill statuspage, int value)
    {
        //ーーーーーーーーーーーーーーーースキルカードーーーーーーーーーーーーーーーー
        GameObject CL_Page = Instantiate(SkillPage);
        SkillPageManager pageManager = CL_Page.GetComponent<SkillPageManager>();

        foreach (var playerskill in playerController.skillINFO) 
        {
            if (statuspage.skill == playerskill.skillDATA) 
            { 
                pageManager.LevelUpTexts.SetActive(true);
                pageManager.LevelUp = true;
            }
        }

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
        pageManager.CoolTimeTEXT.text = statuspage.skill.CoolTime.ToString();
        pageManager.TotalTEXT.text = statuspage.skill.TotalDamage.ToString();

        rect.transform.position = new Vector2(-1200, 0);

        //設置間隔 -500 0 500
        //500 1000 1500
        float TargetPos = SkillPagesDistanse * value + SkillPageStartPos;
        //Debug.Log(TargetPos);
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
    public float AdditiveATK = 0;
    public float ConfusionTime = 0;
    public float Defense = 0;
    public float HP = 0;
    public float Regen = 0;
    public float RecoveryHP =0;
    public float Critical = 0;
    public float Range = 0;
    public float Speed = 0;
    public int RandomLevelUp = 0;
    public float Coin = 0;
    
}
[System.Serializable]
public class Relic 
{
    public string Name;
    public RelicData data;
     string StatusName = "遺物獲得";
    
    
}
