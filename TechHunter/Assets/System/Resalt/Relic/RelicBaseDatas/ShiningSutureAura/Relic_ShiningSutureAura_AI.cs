using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_ShiningSutureAura_AI : RelicBase_AI
{

    [SerializeField] float RandomRecovery = 10;

    [SerializeField] BuffData buffData;
    [SerializeField] AudioClip clip;
    [SerializeField] GameObject Effect;
    [SerializeField] ParticleSystem Effect_Relic;
    
    ALL_SystemManager systemManager => ALL_SystemManager.Instance;
    AudioManager audioManager => AudioManager.instance;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnPlayerDamage += OnDamage;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerDamage -= OnDamage;
    }

    void RecoveryCoolTime()
    {
        foreach (var skill in playerController.skillINFO)
        {
            skill.SkillCardManager.isCoolTime_add(3);
        }
    }
    public void OnDamage(float damage, bool Invincible)
    {
        float playerDiffence = (playerController.AddAtkDF + playerController.playerBuff.DEF_AllBuff) * playerController.playerBuff.MultiplyDEF_AllBuff;
        float allDamage = damage - playerDiffence;
        if (allDamage < 1) 
        { 
        allDamage = 1;
        }
        if (Invincible ) return;
        RecoveryCoolTime();
        if (allDamage <= 0) return;
        
        float random = Random.Range(0, 100);
        if (random < RandomRecovery) 
        {
            Effect.SetActive(true);
            Effect_Relic.Play();
            playerController.isRecoveryHP(allDamage);
        }
        
    }
   
}