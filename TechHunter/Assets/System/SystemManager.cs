using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour
{
    [SerializeField] EnemySpawnBase enemySpawn;
    [SerializeField] system_GameModeManager gameModeManager;
    [SerializeField] sytem_GameSpotsController sytem_GameSpotsController;
    [SerializeField] bool isResetFlag = false;
    [SerializeField] bool isGameOverMove = false;
    public SkillCardData NoneSkill;
    public Animator shopWindowAnimator;
    
    public int stageNumber =0;

    public List<CoinManager> coinManagers = new List<CoinManager>();
 

    public List<GameObject> AllEnemy = new List<GameObject>();
    PlayerController controller => PlayerController.Instance;
    public int Level = 0;
    public float WaitEnemyDieCount =0;
    public bool isMinEnemy = true;
    public static SystemManager Instance;

    SkillController skillController;

    float StartCount = 0;
    float ResaltCount=0;
    public enum ModeType
    {
        M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24,
    }
    public ModeType mode = ModeType.M1;

    public enum GameSystemMODE 
    { 
    
        Debug,
        Build
    
    }
    public GameSystemMODE gameSystemMode = GameSystemMODE.Debug;
    public enum GameMode
    {

        Nomal,
        Start,
        Result,
        Goal,

        None,
        Movie

    }
    public GameMode gameMode = GameMode.Nomal;

    public enum Stage
    {

        Ferust,
        Diana,
        Garewo,
        Kardia,
        Pastal,
        Teruseto,
        Shuela,
        Gratohto,
        LAST

    }
    public List<Stage> nextStage = new List<Stage>();
    public Stage stage = Stage.Ferust;
    void Awake()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        skillController = GetComponent<SkillController>();
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void OnEnable()
    {
        GlobalEvents.OnPlayerDie += OnPlayerDie;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerDie -= OnPlayerDie;
    }
    public void OnPlayerDie(float damage)
    {
        if (!isGameOverMove) return;
        SceneManager.LoadScene("GameOverScene");
    }

    // Update is called once per frame
    void Update()
    {
        AllEnemy.RemoveAll(enemy => enemy == null);
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド後の実行ファイルを終了
        Application.Quit();
#endif
        }
        if (gameMode == GameMode.Nomal)
        {
            if (WaitEnemyDieCount < 1)
            {
                if(gameMode != GameMode.Movie)
                WaitEnemyDieCount += Time.deltaTime;
            }
            else
            {
                if (AllEnemy.Count == 0)
                {
                    WaitEnemyDieCount = 0;
                    //Debug.Log("全員死んだ！");
                    gameMode = GameMode.Result;
                    mode = ModeType.M1;
                }
                if (AllEnemy.Count >= 5)
                {
                    isMinEnemy = true;
                }
                else 
                { 
                    isMinEnemy = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && isResetFlag) 
        {
            SceneManager.LoadScene("SampleScene");

        }
    }
    private void FixedUpdate()
    {
        switch (gameMode)
        {

            case GameMode.Nomal:

                break;
            case GameMode.Start:
            isStart();
                break;
            case GameMode.Goal:

                break;
            case GameMode.Result:
                isResult();
                break;

        }
    }
    void isGoal() 
    { 
    
    }
    public void AllSkillReset() 
    {
        foreach (SkillInfo skill in controller.skillINFO)
        {            
                skill.skillDATA = NoneSkill;      
        }
    }
    public void NextStageReset() 
    {
        sytem_GameSpotsController.PlayerSystemPos = new Vector2(-1, 0);

        controller.transform.position = Vector2.zero;
        gameModeManager.BattlePhase = 0;
        gameModeManager.GameLevel = 0;
        mode = ModeType.M1;
        gameMode = GameMode.None;
    }
    public void ReStart() 
    {
        controller.AddATK = 0;
        controller.AddAtkDF = 0;
        controller.AddCritical = 0;
        controller.Add_AdditiveATK = 0;
        controller.AddHP = 0;
        controller.AddMultiplierATK = 1;
        controller.AddRange = 0;
        controller.AddRegen = 0;
        controller.AllCoins = 0;
        controller.isRecoveryHP(99999999);

        sytem_GameSpotsController.PlayerSystemPos = new Vector2(-1,0);

        controller.transform.position = Vector2.zero;

        int i = 0;
        foreach (SkillInfo skill in controller.skillINFO) //通常攻撃意外削除
        {
            bool RemoveSkill = true;
            foreach (SkillCardData NomalSkill in controller.NomalSkillData) 
            {
                if (skill.skillDATA == NomalSkill)
                {
                    
                    RemoveSkill = false;
                    break;
                }
                
            }if (RemoveSkill) skill.skillDATA = NoneSkill;
        }

        controller.isAddStatus();
        gameModeManager.BattlePhase = 0;
        gameModeManager.GameLevel = 0;
        mode = ModeType.M1;
        gameMode = GameMode.None;
    }

    void isResult() 
    { 
        if(mode == ModeType.M1) 
        {

            skillController.isStartPageChoice();

            foreach (CoinManager coin in coinManagers) 
            {
                coin.moveType = CoinManager.MoveType.Chase;
            }
            coinManagers.Clear();
            mode = ModeType.M2;
            controller.rb.velocity = Vector2.zero;
            controller.movetype = PlayerController.MoveType.Wait;
        }
        if (mode == ModeType.M2)
        { 
        
        }
        if (mode == ModeType.M3)
        {

        }
    }
    void isStart()//ゲームの開始
    {
        foreach (SkillInfo skill in controller.skillINFO) 
        {//クールタイム全回復
            skill.SkillCardManager.isCoolTime_add(skill.SkillCardManager.CoolTime);
            if (skill.SkillCardManager.MaxStack > 0)
            {
                skill.SkillCardManager.CountStack = skill.SkillCardManager.MaxStack;
                skill.SkillCardManager.isCoolTime_add(-100);
            }
        }

        StartCount += Time.deltaTime;
        if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.BossBattleSpot)
        {
            if (mode == ModeType.M1)
            {


                enemySpawn.isBossMapSpawn();

                //controller.isStartPointMove();
                controller.movetype = PlayerController.MoveType.NoAction;
                mode = ModeType.M2;
                StartCount = 0;
                //Debug.Log("呼び出された");
            }
            if (mode == ModeType.M2)
            {

                if (StartCount > 0.05)
                {
                    controller.isStartPointMove(enemySpawn.Seve_TileMap_C.StartPoint);

                    mode = ModeType.M3;
                    StartCount = 0;
                }
            }
            if (mode == ModeType.M3)
            {
                controller.MoveInput = new Vector2(0, 1);
                if (StartCount > 1)
                {
                    controller.MoveInput = new Vector2(0, 0);
                    //enemySpawn.isStartClone();
                    mode = ModeType.M1;
                    StartCount = 0;
                    gameMode = GameMode.Nomal;
                    controller.movetype = PlayerController.MoveType.Nomal;
                }
            }
        }
        else if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.TradeSpot) 
        {
            if (mode == ModeType.M1)
            {
                enemySpawn.isShopMapSpawn();

                //controller.isStartPointMove();
                controller.movetype = PlayerController.MoveType.NoAction;
                mode = ModeType.M2;
                StartCount = 0;
                //Debug.Log("呼び出された");
            }
            if (mode == ModeType.M2)
            {

                if (StartCount > 0.05)
                {
                    controller.isStartPointMove(enemySpawn.Seve_TileMap_C.StartPoint);

                    mode = ModeType.M3;
                    StartCount = 0;
                }
            }
            if (mode == ModeType.M3)
            {
                controller.MoveInput = new Vector2(0, 1);
                if (StartCount > 1)
                {
                    controller.MoveInput = new Vector2(0, 0);
                    mode = ModeType.M1;
                    StartCount = 0;
                    gameMode = GameMode.Goal;
                    gameModeManager.BattlePhase = 3;
                    controller.movetype = PlayerController.MoveType.Nomal;
                }
            }
        }
        else
        {
            if (mode == ModeType.M1)
            {


                enemySpawn.isMapSpawn();

                //controller.isStartPointMove();
                controller.movetype = PlayerController.MoveType.NoAction;
                mode = ModeType.M2;
                StartCount = 0;
                //Debug.Log("呼び出された");
            }
            if (mode == ModeType.M2)
            {

                if (StartCount > 0.05)
                {
                    controller.isStartPointMove(enemySpawn.Seve_TileMap_C.StartPoint);

                    mode = ModeType.M3;
                    StartCount = 0;
                }
            }
            if (mode == ModeType.M3)
            {
                controller.MoveInput = new Vector2(0, 1);
                if (StartCount > 1)
                {
                    controller.MoveInput = new Vector2(0, 0);
                    enemySpawn.isStartClone();
                    mode = ModeType.M1;
                    StartCount = 0;
                    gameMode = GameMode.Nomal;
                    controller.movetype = PlayerController.MoveType.Nomal;

                }
            }
        }

    }
    public void isGameStart() 
    {
        skillController.ReRollCount = 0;
        controller.animator.SetBool("Dodge", false);
       
        if (gameModeManager.BattlePhase < 3)
        {
            GlobalEvents.InvokeOnNextSpot(gameModeManager.BattlePhase);
            controller.isRecoveryHP(controller.Regen);
            if (enemySpawn.Seve_TileMap_C != null)
            {
                Destroy(enemySpawn.Seve_TileMap_C.gameObject);
                enemySpawn.tilePosition.Clear();
            }
            
            if (gameModeManager.gameMode == system_GameModeManager.AdventureGameMode.BossBattleSpot && gameModeManager.BattlePhase == 1)
            {
                stageNumber++;

                stage = nextStage[stageNumber];
                
                if (stage == Stage.LAST)
                {
                    sytem_GameSpotsController.isLastNextSpotStart();
                }
                else 
                { 
                    sytem_GameSpotsController.isNext_GameSpotStart();
                }
                
            }
            else 
            {
                if (stage == Stage.LAST && gameModeManager.BattlePhase == 1)
                {
                    nextPhase();
                }
                else
                {
                    gameModeManager.BattlePhase++;
                    gameMode = GameMode.Start;
                }
            }      
        }
        else 
        {
            nextPhase();
        }
    }
    void nextPhase() 
    {
        gameModeManager.GameLevel++;
        gameModeManager.BattlePhase = 0;
        AllEnemy.Clear();

        controller.transform.position = Vector3.zero;

        sytem_GameSpotsController.PopSpots();
    }
    public void FinishPhase() 
    {
        gameModeManager.BattlePhase = 0;
        gameModeManager.allgamemode = system_GameModeManager.AllGameMode.SpotChoiceMode;
    }
}
