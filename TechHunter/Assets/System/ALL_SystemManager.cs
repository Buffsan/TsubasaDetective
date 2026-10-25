using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ALL_SystemManager : MonoBehaviour
{
    public SystemManager systemManager;
    public system_GameModeManager system_GameModeManager;
    public sytem_GameSpotsController sytem_GameSpotsController;
    public System_GameStart_Controller system_GameStartController;

    public SkillController skillController;
    public EnemySpawnBase enemySpawnBASE;
    public Camera_Controller camera_Controller;

    public PlayerController playerController;
    public CursorController cursorController;

    public System_EventObjects system_EventObjects;

    public CoinPool coinPool;

    [Header("Dialogue System")]
    [Tooltip("Permanent DialogueRuntimeBootstrap under -----GameSysem-----.")]
    public DialogueRuntimeBootstrap dialogueRuntimeBootstrap;
    [Tooltip("Actual dialogue UI controller, automatically registered during Play Mode.")]
    public DialogueUIController dialogueUIController;
    [Tooltip("Character database shared by shops, NPCs and story events.")]
    public DialogueCharacterDatabase dialogueCharacterDatabase;

    public static ALL_SystemManager Instance;

    public void RegisterDialogueSystem(DialogueRuntimeBootstrap bootstrap,
        DialogueUIController controller, DialogueCharacterDatabase characters)
    {
        if (bootstrap != null) dialogueRuntimeBootstrap = bootstrap;
        if (controller != null) dialogueUIController = controller;
        if (characters != null) dialogueCharacterDatabase = characters;
    }

    public void UnregisterDialogueSystem(DialogueRuntimeBootstrap bootstrap)
    {
        if (dialogueRuntimeBootstrap != bootstrap) return;
        dialogueRuntimeBootstrap = null;
        dialogueUIController = null;
    }

    private void Start()
    {
        // Awake order is unspecified. Start runs after all active Awake calls.
        if (dialogueRuntimeBootstrap == null)
            dialogueRuntimeBootstrap = FindObjectOfType<DialogueRuntimeBootstrap>();

        if (dialogueRuntimeBootstrap != null)
            RegisterDialogueSystem(dialogueRuntimeBootstrap,
                dialogueRuntimeBootstrap.DialogueController,
                dialogueRuntimeBootstrap.CharacterDatabase);
        else if (DialogueUIController.Instance != null)
            dialogueUIController = DialogueUIController.Instance;
    }

    private void Awake()
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
