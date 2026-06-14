using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEndPointArrow : MonoBehaviour
{

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    [SerializeField] GameObject AnimBody;
    [SerializeField] TileMap_Controller Save_TileMap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (ALL_System.systemManager.gameMode) 
        {

            case SystemManager.GameMode.Goal:
                SetMode();
                break;

            default:
                StayMode();
                break;
        
        }
    }

    void SetMode()
    {
        if (ALL_System.enemySpawnBASE.Seve_TileMap_C == null) return;
        AnimBody.SetActive(true);
        if (Save_TileMap == null)
        {
            Save_TileMap = ALL_System.enemySpawnBASE.Seve_TileMap_C;
        }
        else
        {
            Vector2 Direction = Save_TileMap.Goal.transform.position - ALL_System.playerController.transform.position;
            transform.up = Direction.normalized;
        }
    }
    void StayMode() 
    {
        AnimBody.SetActive(false);
    }
}
