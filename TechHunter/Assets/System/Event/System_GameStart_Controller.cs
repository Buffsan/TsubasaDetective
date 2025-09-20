using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class System_GameStart_Controller : MonoBehaviour
{

    [SerializeField]int Phase = 0;

    ALL_SystemManager aLL_System => ALL_SystemManager.Instance;
    PlayerController playerController;
    SkillController skillController;
    sytem_GameSpotsController gameSpotsController;
    SystemManager systemManager;

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Phase == 1) 
        {
            aLL_System.skillController.isAll_StartSkillPageChoice(0);
            NextPhase();
        }
        if (Phase == 3) 
        {
            aLL_System.skillController.isAll_StartSkillPageChoice(1);
            NextPhase();
        }
        if (Phase == 5) 
        {
            aLL_System.sytem_GameSpotsController.isGameSpotStart();
            Phase = 0;
        }
    }
    public void NextPhase() 
    {
        if (Phase != 0)
        {
            Phase++;
        }
    }

    void StartGame() 
    {
        aLL_System.systemManager.AllSkillReset();
        Phase = 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            StartGame();
        }   
    }
}
