using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_bloodSpear : RelicBase_AI
{
    [SerializeField] GameObject SecondAttack;
    [SerializeField] GameObject thirdAttack;
    [SerializeField] float CoolTime = 0.5f;
    [SerializeField] List<BuffData> buffDatas = new List<BuffData>();
    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    [SerializeField] BuffData buffsataWeek;
    [SerializeField] GameObject Effect;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    public GameObject Player;
   
    float CoolCount = 0;
    bool DashAttack = false;
    [SerializeField] float doubleTapTime = 0.4f; // ダブルタップとして認識する猶予時間
    private float timer = 0f;
    private int tapCount = 0;
    private bool inputHeld = false; // 入力を押し続けているかどうか
    private Vector2 prevInput;      // 前フレームの入力
    private Vector2 lastTapDir;

    bool SaveSkill = false;

    Vector2 lastDIR = Vector2.zero;
    [SerializeField] float DashSpeed;
    float DashCount = 0;
    [SerializeField]float DashTimer = 0.1f;
    public override void BaseAction()
    {

        
        if (lastDIR != Vector2.zero) 
        {
            Debug.Log(lastDIR);
            playerController.isSpecialMove(DashSpeed, lastDIR ,ALL_System.playerController.gameObject);
            if (DashCount > DashTimer && !DashAttack && !playerController.SaveSkillBase || DashCount > DashTimer*2 && DashAttack && !playerController.SaveSkillBase || playerController.SaveSkillBase && DashCount > DashTimer * 2)
            {
                DashAttack = false;
                DashCount = 0;
                lastDIR = Vector2.zero;
            }
            else 
            {
                
                DashCount += Time.fixedDeltaTime;
            }
        }
        if (playerController.movetype == PlayerController.MoveType.Dodge) 
        { 
        DashAttack = true;
        }

            // タイマーを減らす
            if (timer > 0)
            timer -= Time.deltaTime;

        Vector2 currentInput = playerController.MoveInput;

        if (CoolCount > CoolTime)
        {
            // 「入力がなかった → 入力が入った」瞬間を検知（押された瞬間）
            bool inputJustPressed = (prevInput == Vector2.zero && currentInput != Vector2.zero);

            // 「入力が入った → 離された」瞬間を検知（離された瞬間）※必要なら使用
            // bool inputJustReleased = (prevInput != Vector2.zero && currentInput == Vector2.zero);

            if (inputJustPressed)
            {
                if (tapCount == 0)
                {
                    tapCount = 1;
                    lastTapDir = currentInput.normalized;
                    timer = doubleTapTime;
                }
                else if (tapCount == 1 && timer > 0)
                {
                    // 同じ方向ならダブルタップ成立
                    if (Vector2.Dot(lastTapDir, currentInput.normalized) > 0.9f)
                    {
                        lastDIR = lastTapDir;
                        Debug.Log("同方向ダブルタップ!");
                        CoolCount = 0;

                        if (playerController.SaveSkillBase)
                        {
                            foreach (var buffs in buffDatas)
                            {
                                playerController.playerBuff.SpawnBuff(buffs);
                            }
                            GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                            GameObject CL_Attack = Instantiate(thirdAttack, transform.position, Quaternion.identity);
                            CL_Attack.transform.up = lastTapDir.normalized;
                            CL_Attack.transform.parent = playerController.transform;
                            CL_Effect.transform.up = lastTapDir.normalized;
                            CL_Effect.transform.parent = playerController.transform;
                            Destroy(CL_Attack, 0.35f);
                            Destroy(CL_Effect, 1);
                            playerController.Dash = true;
                            AudioPlay(audioClips[1]);

                        }
                        else
                        {
                            if (DashAttack)
                            {
                                foreach (var buffs in buffDatas)
                                {
                                    playerController.playerBuff.SpawnBuff(buffs);
                                }
                                GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                                GameObject CL_Attack = Instantiate(SecondAttack, transform.position, Quaternion.identity);
                                CL_Attack.transform.up = lastTapDir.normalized;
                                CL_Attack.transform.parent = playerController.transform;
                                CL_Effect.transform.up = lastTapDir.normalized;
                                CL_Effect.transform.parent = playerController.transform;
                                Destroy(CL_Attack, 0.35f);
                                Destroy(CL_Effect, 1);
                                playerController.Dash = true;
                                AudioPlay(audioClips[1]);
                            }
                            else
                            {
                                playerController.playerBuff.SpawnBuff(buffsataWeek);
                                GameObject CL_Attack = Instantiate(AttackObject, transform.position, Quaternion.identity);
                                CL_Attack.transform.up = lastTapDir.normalized;
                                CL_Attack.transform.parent = playerController.transform;
                                Destroy(CL_Attack, 0.35f);
                                playerController.Dash = true;
                                AudioPlay(audioClips[0]);
                            }
                        }

                    }
                    tapCount = 0;
                    timer = 0;
                }
            }
        }
        else 
        { 
        CoolCount += Time.fixedDeltaTime;
        }

        // 時間切れでリセット
        if (timer <= 0)
            tapCount = 0;

        // 次のフレームのために記録
        prevInput = currentInput;
    }
}
