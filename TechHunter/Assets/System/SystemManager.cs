using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    [SerializeField] EnemySpawnBase enemySpawn;
    [SerializeField] system_GameModeManager gameModeManager;
    [SerializeField] sytem_GameSpotsController sytem_GameSpotsController;
    [SerializeField] SkillCardData NoneSkill;
 

    public List<GameObject> AllEnemy = new List<GameObject>();
    PlayerController controller => PlayerController.Instance;
    public int Level = 0;
    float WaitEnemyDieCount =0;
    public static SystemManager Instance;

    SkillController skillController;

    float StartCount = 0;
    float ResaltCount=0;
    public enum ModeType
    {
        M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12, M13, M14, M15, M16, M17, M18, M19, M20, M21, M22, M23, M24,
    }
    public ModeType mode = ModeType.M1;
    public enum GameMode
    {

        Nomal,
        Start,
        Result,
        Goal,

        None

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
        Shuela

    }
    public Stage stage = Stage.Ferust;
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        AllEnemy.RemoveAll(enemy => enemy == null);
        if (gameMode == GameMode.Nomal)
        {
            if (WaitEnemyDieCount < 1)
            {
                WaitEnemyDieCount += Time.deltaTime;
            }
            else
            {
                if (AllEnemy.Count == 0)
                {
                    WaitEnemyDieCount = 0;
                    Debug.Log("全員死んだ！");
                    gameMode = GameMode.Result;
                    mode = ModeType.M1;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) 
        {
            //isGameStart();
 
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

    public void ReStart() 
    {
        controller.AddATK = 0;
        controller.AddAtkDF = 0;
        controller.AddCritical = 0;
        controller.AddHP = 0;
        controller.AddMultiplierATK = 1;
        controller.AddRange = 0;
        controller.AddRegen = 0;
        controller.AllCoins = 0;
        controller.isRecoveryHP(99999999);

        sytem_GameSpotsController.PlayerSystemPos = new Vector2(-1,0);

        controller.transform.position = Vector2.zero;

        int i = 0;
        foreach (SkillInfo skill in controller.skillINFO) 
        {
            if (i != 0 && i != 1) 
            {
                skill.skillDATA = NoneSkill;
            }
                i++;
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
    void isStart()
    {

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
    {skillController.ReRollCount = 0;
        if (gameModeManager.BattlePhase < 3)
        {
            
            gameModeManager.BattlePhase++;
            gameMode = GameMode.Start;
            controller.isRecoveryHP(controller.Regen);
            if (enemySpawn.Seve_TileMap_C != null)
            {
                Destroy(enemySpawn.Seve_TileMap_C.gameObject);
                enemySpawn.tilePosition.Clear();
            }
        }
        else 
        {
            gameModeManager.GameLevel++;
            gameModeManager.BattlePhase =0;

            controller.transform.position = Vector3.zero;

            sytem_GameSpotsController.PopSpots();
        }
    }
    public void FinishPhase() 
    {
        gameModeManager.BattlePhase = 0;
        gameModeManager.allgamemode = system_GameModeManager.AllGameMode.SpotChoiceMode;
    }
}
