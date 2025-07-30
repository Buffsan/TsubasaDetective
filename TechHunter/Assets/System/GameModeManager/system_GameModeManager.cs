using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class system_GameModeManager : MonoBehaviour
{
    // Start is called before the first frame update

    PlayerController playerController => PlayerController.Instance;
    public int BattlePhase = 0;

    public int GameLevel = 0;
    public int AllGameLevel;
    public int SatgeNumber = 0;
    public enum AdventureGameMode 
    { 
    
        NomalBattleSpot,
        EliteBattleSpot,
        MortalBattleSpot,
        BossBattleSpot,
        TradeSpot,
        StartSpot,

        None,

    }
    public AdventureGameMode gameMode = AdventureGameMode.None;

    public enum AllGameMode 
    {

        AdventureGameMode,
        SpotChoiceMode

    }
    public AllGameMode allgamemode = AllGameMode.AdventureGameMode;

    public enum SpotChoisePhase 
    { 
    
        BeforeChoise,//進める区域を探す
        Choise,//進める区域を選択する
        AfterChoise//区域の選択を確定する

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (gameMode == AdventureGameMode.NomalBattleSpot)
        {
            AllGameLevel = GameLevel;
        }
        else if (gameMode == AdventureGameMode.EliteBattleSpot)
        {
            AllGameLevel = GameLevel +1;
        }

        switch (allgamemode) 
        {
            case AllGameMode.AdventureGameMode:

            break;
                case AllGameMode.SpotChoiceMode: 
                playerController.movetype = PlayerController.MoveType.NoAction;
                break;
        
        
        }
        if (allgamemode == AllGameMode.SpotChoiceMode) 
        {
            
        }
    }
}
