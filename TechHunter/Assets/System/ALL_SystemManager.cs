using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ALL_SystemManager : MonoBehaviour
{
    public SystemManager systemManager;
    public system_GameModeManager system_GameModeManager;
    public sytem_GameSpotsController sytem_GameSpotsController;

    public SkillController skillController;
    public EnemySpawnBase enemySpawnBASE;
    public Camera_Controller camera_Controller;

    public PlayerController playerController;
    public CursorController cursorController;

    public System_EventObjects system_EventObjects;

    public static ALL_SystemManager Instance;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
}
