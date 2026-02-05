using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Relic_AncientDefibrillator : RelicBase_AI
{

    [SerializeField] bool useRelic = false;
    [SerializeField] BuffData buffData;
    [SerializeField] AudioClip clip;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Effect_Relic;
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
        GlobalEvents.OnNextSpot += OnNextSpot;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerDamage -= OnDamage;
        GlobalEvents.OnNextSpot -= OnNextSpot;
    }

    void RecoveryCoolTime() 
    {
        foreach (var skill in playerController.skillINFO) 
        {
            skill.SkillCardManager.isCoolTime_add(2.5f);
        }
    }
    public void OnDamage(float damage, bool Invincible) 
    {
        
        if (damage <= 0 || Invincible) return; 
        RecoveryCoolTime();
        if (!useRelic)
        {
            if (playerController.MaxHP * 0.4f > playerController.HP - damage)
            {

                useRelic = true;
                playerController.isRecoveryHP(playerController.MaxHP * 0.3f);
                playerController.playerBuff.SpawnBuff(buffData);
                systemManager.camera_Controller.Shake(0.2f, 0.4f);
                audioManager.PlaySE(clip);
                GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_Effect, 8);
                GameObject CL_Effect1 = Instantiate(Effect_Relic, transform.position, Quaternion.identity);
                Destroy(CL_Effect1, 2);
            }
        }
    }
    public void OnNextSpot(int spot) 
    {
        Debug.Log("ReLLLLLLLLLLLLLic");
        useRelic = false;
    }
   
}
