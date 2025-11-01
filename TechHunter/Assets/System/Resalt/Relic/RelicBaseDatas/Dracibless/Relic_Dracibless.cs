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
    [SerializeField]float CoolTimeRecoveryCount = 0;
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
            || playerController.MoveInput != Vector2.zero) { isSpaceInput = false;}
    }

    public float CoolTimeAgein() 
    {
        float Score = 0;
        if (isSpaceInput)
        {
            Score += Time.fixedDeltaTime *2;
        }
        else 
        {
            Score += Time.fixedDeltaTime;
        }

        return Score;
    }
    public override void BaseAction()
    {

        if (playerController.MoveInput == Vector2.zero) 
        {
            
            CoolTimeRecoveryCount += CoolTimeAgein();
            if (CoolTimeRecoveryCount > 1)
            {

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
                

                CoolTimeRecoveryCount = 0;

                if (SaveCard != null)
                {
                    SaveCard.isCoolTime_add(3);
                    ALL_System.camera_Controller.Shake(0.1f, 0.05f);
                    audioManager.isPlaySE(AudioClips[0]);
                    GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                    Destroy(CL_Effect, 3);
                }
            }
        }

        if (!isSpaceInput) { ResetCount(); return;}

        WaitCount += Time.fixedDeltaTime;
       
    }
    void ResetCount() 
    {
        particleSystem.emissionRate = 0;
        if (WaitCount >= 1) {
            playerController.playerBuff.SpawnBuff(buffData);
            audioManager.isPlaySE(AudioClips[1]);
            CoolTimeRecoveryCount = 99;
        }

        WaitCount = 0; 
        
    }
}
