using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TheOrgelFoEternalAgony_AI : RelicBase_AI
{

    [SerializeField] bool useRelic = false;
    [SerializeField] BuffData buffData;
    [SerializeField] AudioClip clip;
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Effect_Relic;
    [SerializeField] ParticleSystem particleSystem;

    [SerializeField] float AttackScale = 1.2f;

    int BuffStack = 0;
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
        GlobalEvents.OnSkillUse += OnSkill;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerDamage -= OnDamage;
        GlobalEvents.OnSkillUse -= OnSkill;

    }

    private void OnSkill(PlayerSkillBase skillBase, SkillCardData AI)
    {
        if (BuffStack <= 0) return;
        for (int i = 0; i < BuffStack; i++) 
        {
            playerController.playerBuff.SpawnBuff(buffData);

            StartCoroutine(LateAttack((0.9f / BuffStack) * (float)i));
        }
        BuffStack = 0;
        particleSystem.emissionRate = BuffStack;
    }

    public void OnDamage(float damage, bool Invincible)
    {
        BuffStack++;
        particleSystem.emissionRate = BuffStack;
    }
    IEnumerator LateAttack(float WaitTime) 
    { 
    
        yield return new WaitForSeconds(WaitTime);

        GameObject CL_ATTACK = Instantiate(AttackObject, transform.position, Quaternion.identity);
        CL_ATTACK.transform.up = playerController.CursorDirection;
        int random = Random.Range(0, 2);
        if (random == 0)
        {
            CL_ATTACK.transform.localScale = new Vector2(AttackScale, AttackScale);
        }
        else 
        {
            CL_ATTACK.transform.localScale = new Vector2(-AttackScale, AttackScale);
        }
        Destroy(CL_ATTACK,1);
    }
}
