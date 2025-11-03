using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_Type_III_Bio_EquipmentGreatLionheadClock : RelicBase_AI
{
    [SerializeField] BuffData buffData;

    [SerializeField] bool Doge = false;

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
        bool useBuff = false;
        foreach (var buff in playerController.playerBuff.playerBuffs) 
        {
            if (buff.buffData == buffData) { Destroy(buff.gameObject); useBuff = true; }
        }
        if (!useBuff) return;
        Reset();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !playerController.SaveSkillBase) 
        {
            bool useBuff = false;
            foreach (var buff in playerController.playerBuff.playerBuffs)
            {
                if (buff.buffData == buffData) useBuff = true;
            }
            if (!useBuff) { playerController.playerBuff.SpawnBuff(buffData); playerController.animator.SetBool("madDoge", true); Doge = true; }
           
        }
        
    }
    private void Reset()
    {
        playerController.animator.SetBool("madDoge", false); 
        Doge = false;
        for (int i = 0; i < 2; i++)
        {
            NeedleSpawnLing(0.5f+i, 0);
        }
    }
    public override void BaseAction()
    {
        if (!Doge) return;
        bool useBuff = false;
        foreach (var buff in playerController.playerBuff.playerBuffs)
        {
            if (buff.buffData == buffData) useBuff = true;
        }
        if (!useBuff) { Reset(); }
    }
    public void SpawnRockNeedle(Vector2 pos)
    {
        GameObject CL_Rock = Instantiate(AttackObject, pos, Quaternion.identity);
        Destroy(CL_Rock, 5);
    }
    private void NeedleSpawnLing(float Lange, int Plass)
    {
        
        for (int i = 0; i < 5 + Plass; i++)
        {

            float angle = (360 / 5 + Plass) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = transform.position.x + 2 * Lange * Mathf.Cos(angleRad);
            float newY = transform.position.y + 2 * Lange * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            SpawnRockNeedle(newPosition);

        }
    }
}
