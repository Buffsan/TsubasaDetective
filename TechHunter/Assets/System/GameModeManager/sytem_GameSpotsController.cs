using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static system_GameModeManager;

public class sytem_GameSpotsController : MonoBehaviour
{

    
    public List<GameSpot_SpotInfo> InputSpot = new List<GameSpot_SpotInfo>();
    [SerializeField] GameSpot_SpotInfo NowSpot;
    public List<GameSpot_SpotInfo> NextInputSpot = new List<GameSpot_SpotInfo>();

    [Space]
    public List<GameSpot_SpotInfo> SpotInfos = new List<GameSpot_SpotInfo>();
    public List<GameObject> DestroyList = new List<GameObject>();
    [SerializeField] sytem_GameSpotsController gamespotcontroller;
    [SerializeField] system_GameModeManager gamemode;
    [SerializeField] SystemManager systemmanager;
    [SerializeField] GameObject SpotPrefab;
    [SerializeField] GameObject PlayerSpotIMage;
    [SerializeField] GameObject CursorPrefab;
    public Vector2 PlayerSystemPos = Vector2.zero;
    

    [SerializeField] float Xdistance = 2;
    [SerializeField] float Ydistance = 1;
    Vector2 StartPos = new Vector2 (-4.5f, -3.8f);

    ALL_SystemManager ALL_SystemManager => ALL_SystemManager.Instance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gamemode.allgamemode == AllGameMode.SpotChoiceMode)
        {
            CursorPrefab.SetActive(true);
        }
        else 
        {
            CursorPrefab.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.J)) 
        {

            systemmanager.ReStart();
            gamemode.allgamemode = AllGameMode.SpotChoiceMode;
            DestroyInputSpots();
            UpdateInputSpot();
            NewSpawnSpots();
        }
        if (Input.GetKeyDown(KeyCode.K)) 
        {
           //PopSpots();
        }
            PlayerSpot();


    }
    public void PopSpots() 
    {
        gamemode.allgamemode = AllGameMode.SpotChoiceMode;

        
        NewSpawnSpots();
    }
    void PlayerSpot() 
    {
        NextInputSpot.Clear();

        if (PlayerSystemPos.x == -1)
        {
            PlayerSpotIMage.transform.position = new Vector2(StartPos.x + PlayerSystemPos.x * Xdistance, 0);
            if (InputSpot != null)
            {
                foreach (GameSpot_SpotInfo next in InputSpot)
                {
                    if (next.gameMode == GameSpot_SpotInfo.AdventureGameMode.StartSpot)
                    {
                        NowSpot = next;
                    }
                }
                foreach (GameSpot_SpotInfo next in InputSpot)
                {
                    if (next.SpotPos.x == 0)
                    {
                        NextInputSpot.Add(next);
                    }
                }
            }
        }
        else if (PlayerSystemPos.x == 4)
        {
            PlayerSpotIMage.transform.position = new Vector2(StartPos.x + PlayerSystemPos.x * Xdistance, 0);
            if (InputSpot != null)
            {
                foreach (GameSpot_SpotInfo next in InputSpot)
                {
                    if (next.gameMode == GameSpot_SpotInfo.AdventureGameMode.BossBattleSpot)
                    {
                        NowSpot = next;
                    }
                }
                
            }
        }
        else
        {
            
            if (InputSpot != null)
            {
                NextInputSpot.Clear();
                foreach (GameSpot_SpotInfo next in InputSpot)
                {
                    if (PlayerSystemPos == next.SpotPos)
                    {
                        NowSpot = next;
                    }
                }
                foreach (GameSpot_SpotInfo next in InputSpot)
                {
                    if (NowSpot.SpotPos.x == 3)
                    {
                        if (next.gameMode == GameSpot_SpotInfo.AdventureGameMode.BossBattleSpot)
                        {
                            NextInputSpot.Add(next);
                        }
                    }
                    else
                    {
                        if (NowSpot.SpotPos.x + 1 == next.SpotPos.x)
                        {

                            //
                            if (NowSpot.Up && next.SpotPos.y == NowSpot.SpotPos.y + 1)
                            {

                                NextInputSpot.Add(next);
                                continue;
                            }
                            if (NowSpot.Down && next.SpotPos.y == NowSpot.SpotPos.y - 1)
                            {
                                //Debug.Log("現在地d");
                                NextInputSpot.Add(next);
                                continue;
                            }
                            if (NowSpot.Mast && next.SpotPos.y == NowSpot.SpotPos.y)
                            {
                                //Debug.Log("現在地m");
                                NextInputSpot.Add(next);
                                continue;
                            }

                        }
                    }
                    
                }

            }


            PlayerSpotIMage.transform.position = new Vector2(StartPos.x + PlayerSystemPos.x * Xdistance, StartPos.y + PlayerSystemPos.y * Ydistance);
        }


    }
    void DestroyInputSpots() 
    {
        foreach (GameObject destroylists in DestroyList) 
        {
            Destroy(destroylists);
        }
        DestroyList.Clear();
    
    }
    public void UpdateInputSpot() 
    {

        InputSpot.Clear();

        float AllSpawRate = 0;
        foreach (GameSpot_SpotInfo spInfo in SpotInfos) 
        {
            if (spInfo.Skip)
            {
                continue;
            }
            else
            {
                AllSpawRate += spInfo.SpawNumber;
            }
        }
        //Debug.Log (AllSpawRate);

        for (int f = 0; f < 16; f++)
        {
            
            float NextSpawRate = 0; float RandomRate = Random.Range(0, (int)AllSpawRate);
            foreach (GameSpot_SpotInfo spInfo in SpotInfos)
            {   
                

                
                if (spInfo.Skip)
                {
                    float adddd = NextSpawRate + spInfo.SpawNumber;
                    //Debug.Log(NextSpawRate + "<=" + RandomRate + " && " + adddd + ">"+ RandomRate);
                    Debug.Log("Skip :" + RandomRate);
                    
                    NextSpawRate += spInfo.SpawNumber;
                    continue;
                }
                else
                {//Debug.Log("AAAA");
                    if (NextSpawRate <= RandomRate && NextSpawRate + spInfo.SpawNumber > RandomRate)
                    {

                        //Debug.Log("Input :"+RandomRate);
                        float adddd = NextSpawRate + spInfo.SpawNumber;
                        //Debug.Log(NextSpawRate + "<=" + RandomRate + " && " + adddd + ">" + RandomRate);


                        GameSpot_SpotInfo clonedInfo = spInfo.Clone(); ; // DeepCloneメソッドを仮定
                        clonedInfo.Mast = true;
                        InputSpot.Add(clonedInfo); // クローンを追加
                        //InputSpot.Add(spInfo);
                        break;
                    }

                }

                
                NextSpawRate += spInfo.SpawNumber;
            }
        }
        int y = 0;
        int x = 0;
       
        foreach (var spInfo in InputSpot)
        {

           
            int RandomPop = Random.Range(0, 2);
            if (RandomPop == 1 && y != 3 && x != 3)
            {
                spInfo.Up = true;
            }
            RandomPop = Random.Range(0, 2);
            if (RandomPop == 1 && y != 0 && x != 3)
            {
                spInfo.Down = true;
            }
            if (x == 3)
            {
                spInfo.Mast = false;
            }

            spInfo.SpotPos = new Vector2(x, y);
            //Debug.Log("X:"+ x + "Y:" + y);

            y++;
            if (y > 3)
            {
                y = 0;
                x++;
            }
        }
      
    }
    public void RandomCloneSpot(float X,float Y,int Number) 
    {
        GameObject CL_Spot = Instantiate(SpotPrefab, new Vector2(StartPos.x + Xdistance * X, StartPos.y + Ydistance * Y), Quaternion.identity);
        sytem_GameSpot CL_gameSpot = CL_Spot.GetComponent<sytem_GameSpot>();

        InputSpot[Number].SpotPos = new Vector2(X, Y);

        CL_gameSpot.controller = this;
        CL_gameSpot.SpotInfo = InputSpot[Number];
        CL_gameSpot.SpotInfo.SpotPrefab = CL_Spot;
        CL_gameSpot.SpotSystemPos = new Vector2(X, Y);
        CL_gameSpot.Up = InputSpot[Number].Up;
        CL_gameSpot.Down = InputSpot[Number].Down;
        CL_gameSpot.Mast = InputSpot[Number].Mast;
        /*
        int RandomPop = Random.Range(0, 2);
        if (RandomPop == 1 && Y != 3 && X != 3)
        {
            CL_gameSpot.Up = true;
        }
        RandomPop = Random.Range(0, 2);
        if (RandomPop == 1 && Y != 0 && X != 3)
        {
            CL_gameSpot.Down = true;
        }
        if (X == 3)
        {
            CL_gameSpot.Mast = false;
        }*/
        DestroyList.Add(CL_Spot);
    }
    public void CloneSpot(float X, float Y,int Type)
    {
        GameObject CL_Spot = Instantiate(SpotPrefab, new Vector2(StartPos.x + Xdistance * X, StartPos.y + Ydistance * Y), Quaternion.identity);
        sytem_GameSpot CL_gameSpot = CL_Spot.GetComponent<sytem_GameSpot>();

        CL_gameSpot.controller = this;
        CL_gameSpot.SpotSystemPos = new Vector2(X, Y);
        if (Type == 1)
        {
            
            CL_gameSpot.SpotInfo = SpotInfos[4];
            CL_gameSpot.EX_Left = true;
        }
        else if (Type == 0)
        {
            
            CL_gameSpot.SpotInfo = SpotInfos[5];
            CL_gameSpot.EX_Right = true;
            CL_gameSpot.Mast = false;
        }
        InputSpot.Add(CL_gameSpot.SpotInfo);
   
        DestroyList.Add(CL_Spot);
    }
    public void NewSpawnSpots() 
    {
        ALL_SystemManager.system_EventObjects.UI_Active_FALSE();
        //ALL_SystemManager.playerController.movetype = PlayerController.MoveType.Wait;
        int spotInfosNumber = 0;
        int X_spotInfosNumber = 0;
        int t = 0;
        CloneSpot(-1, 1.5f, 0);
        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < 4; i++)
            {
                RandomCloneSpot(X_spotInfosNumber,i,spotInfosNumber);
                spotInfosNumber++;
            }
            X_spotInfosNumber++;
        }
        CloneSpot(4,1.5f,1);
    }
    public void SpawnSpots() 
    {
        int spotInfosNumber = 0;
        int X_spotInfosNumber = 0;
        for (int f = 0; f < 4; f++)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject CL_Spot = Instantiate(SpotPrefab,new Vector2(StartPos.x + Xdistance*X_spotInfosNumber,StartPos.y + Ydistance * i),Quaternion.identity);
                sytem_GameSpot CL_gameSpot = CL_Spot.GetComponent<sytem_GameSpot>();
                CL_gameSpot.SpotInfo = SpotInfos[0];

                InputSpot.Add(CL_gameSpot.SpotInfo);

                spotInfosNumber++;

            }
            X_spotInfosNumber++;
        }
    
    }
    public void SpawnSpot() 
    { 
    
    }
    public void SelextSpot(Vector2 newPos,string name) 
    {
        if (name == AdventureGameMode.NomalBattleSpot.ToString()) 
        {
            gamemode.gameMode = AdventureGameMode.NomalBattleSpot;
        }
        if (name == AdventureGameMode.EliteBattleSpot.ToString())
        {
            gamemode.gameMode = AdventureGameMode.EliteBattleSpot;
        }
        if (name == AdventureGameMode.MortalBattleSpot.ToString())
        {
            gamemode.gameMode = AdventureGameMode.MortalBattleSpot;
        }
        if (name == AdventureGameMode.BossBattleSpot.ToString())
        {
            gamemode.gameMode = AdventureGameMode.BossBattleSpot;
        }
        if (name == AdventureGameMode.TradeSpot.ToString())
        {
            gamemode.gameMode = AdventureGameMode.TradeSpot;
        }

        PlayerSystemPos = newPos;
        systemmanager.isGameStart();
        DestroyInputSpots();
        //ALL_SystemManager.playerController.movetype = PlayerController.MoveType.Wait;
        ALL_SystemManager.system_EventObjects.UI_Active_TRUE();
        gamemode.allgamemode = AllGameMode.AdventureGameMode;
    }
}

[System.Serializable]
public class GameSpot_SpotInfo
{

    public string Name;
    public Sprite SpotImage;
    public Vector2 SpotPos;

    

    public bool Skip = false;

    public bool Up = false;
    public bool Down = false;
    public bool Mast = false;

    public GameObject SpotPrefab;

    public bool LastSpot = false;
    public bool StartSpot = false;

    public int SpawNumber =0;
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
    public GameSpot_SpotInfo Clone()
    {
        return new GameSpot_SpotInfo
        {
            SpawNumber = this.SpawNumber,
            Skip = this.Skip,
            Name = this.Name,                // 新しい項目: Name
            SpotImage = this.SpotImage,      // 新しい項目: SpotImage
            SpotPos = this.SpotPos,          // 新しい項目: SpotPos
            Up = this.Up,                    // 新しい項目: Up
            Down = this.Down,                // 新しい項目: Down
            Mast = this.Mast,                // 新しい項目: Mast
            SpotPrefab = this.SpotPrefab,    // 新しい項目: SpotPrefab
            LastSpot = this.LastSpot,        // 新しい項目: LastSpot
            StartSpot = this.StartSpot,      // 新しい項目: StartSpot
            gameMode = this.gameMode         // 新しい項目: gameMode (enum)

        };
    }

}