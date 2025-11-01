using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Relic_Dracibless : RelicBase_AI
{

    [SerializeField] bool isSpaceInput = false;
    [SerializeField] BuffData buffData;
    [SerializeField] List<AudioClip> AudioClips = new List<AudioClip>();
    [SerializeField] GameObject Effect;
    [SerializeField] ParticleSystem particleSystem;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    float WaitCount = 0;
    float CoolTimeRecoveryCount = 0;
    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnSkillUse += OnSkill;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnSkillUse -= OnSkill;
    }
    private void OnSkill(PlayerSkillBase skillBase, GameObject AI)
    {
        isSpaceInput = false;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { isSpaceInput = true; particleSystem.emissionRate = 50; }
        if (Input.GetKeyUp(KeyCode.Space)
            || playerController.MoveInput != Vector2.zero) isSpaceInput = false;
    }

    public override void BaseAction()
    {
        
        if (!isSpaceInput) { ResetCount(); return;}

        WaitCount += Time.fixedDeltaTime;
        if (WaitCount < 1) return;

        SkillCardManager SaveCard = null;
        float MaxCoolTime = 0;
        foreach (var skills in playerController.skillINFO) 
        {
            if (skills.SkillCardManager.mode == SkillCardManager.Mode.CoolTime && skills.SkillCardManager.CoolTime > MaxCoolTime) 
            { 
                MaxCoolTime = skills.SkillCardManager.CoolTime;
                SaveCard = skills.SkillCardManager;
            }
        }
        CoolTimeRecoveryCount += Time.fixedDeltaTime;
        if (CoolTimeRecoveryCount > 1) 
        { 
            CoolTimeRecoveryCount = 0;

            if (SaveCard != null) { 
                SaveCard.isCoolTime_add(5); 
                ALL_System.camera_Controller.Shake(0.1f, 0.05f);
                audioManager.isPlaySE(AudioClips[0]);
                GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_Effect, 3);
            }
        }
        
    }
    void ResetCount() 
    {
        particleSystem.emissionRate = 0;
        if (WaitCount >= 1) {
            playerController.playerBuff.SpawnBuff(buffData);
            audioManager.isPlaySE(AudioClips[1]);
        }

        WaitCount = 0; 
        CoolTimeRecoveryCount = 0;
    }
}
