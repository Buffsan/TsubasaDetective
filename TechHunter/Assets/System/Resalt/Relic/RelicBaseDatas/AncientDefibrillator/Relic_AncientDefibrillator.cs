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


    public void OnDamage(float damage) 
    {
        if (!useRelic)
        {
            if (playerController.MaxHP * 0.2f > playerController.HP - damage)
            {
                useRelic = true;
                playerController.isRecoveryHP(playerController.MaxHP * 0.2f);
                playerController.playerBuff.SpawnBuff(buffData);
                systemManager.camera_Controller.Shake(0.2f, 0.4f);
                audioManager.isPlaySE(clip);
                GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_Effect, 5);
            }
        }
    }
    public void OnNextSpot(int spot) 
    {
        Debug.Log("ReLLLLLLLLLLLLLic");
        useRelic = false;
    }
   
}
