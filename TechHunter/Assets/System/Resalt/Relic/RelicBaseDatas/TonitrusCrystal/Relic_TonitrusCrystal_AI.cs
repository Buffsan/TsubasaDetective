using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_TonitrusCrystal_AI : RelicBase_AI
{
    [SerializeField] int RecoveryNumber = 50;
    [SerializeField] int Add_RecoveryNumber = 1;
    [SerializeField] int Add_Criticul_RecoveryNumber = 5;
    [SerializeField] float RecoveryHP = 1.0f;
    [SerializeField] int RecoveryCount = 0;

    [SerializeField] float SHOT_LIMIT = 0.1f;
    float shotCount =0;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    [SerializeField] AudioClip audioClip;
    [SerializeField] GameObject Effect;
    AudioManager audioManager => AudioManager.instance;

    private void OnEnable()
    {
        // クリティカルイベントを購読
        GlobalEvents.OnPlayerAttack += OnCritical;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerAttack -= OnCritical;
    }

    private void FixedUpdate()
    {
        if (SHOT_LIMIT > shotCount) 
        {
            shotCount += Time.fixedDeltaTime;
        }
    }

    // クリティカルが発生したときに呼ばれる関数
    private void OnCritical(GameObject target, float damage, float criticalRate, bool c)
    {
        if (damage < 2) return;
        switch (c)
        {
            case true:
                Add(Add_Criticul_RecoveryNumber);
                break;

            case false:
                Add(Add_RecoveryNumber);
                break;
        }
        if (SHOT_LIMIT > shotCount) return;
        if (RecoveryCount < RecoveryNumber) return;
        RecoveryCount = 0;
        shotCount = 0;

        var enemies = ALL_System.systemManager.AllEnemy;
        int count = enemies.Count;
        if (count == 0) return;

        GameObject enemy = null;
        
            var e = enemies[Random.Range(0, count)];
            if (e != null)
            {
                enemy = e;
            }
        
        if (enemy == null) return;
        float angle = Random.Range(-15f, 15f);
        AttackSpawner.Instance.Spawn(
            AttackObject,
            enemy.transform.position,
            Quaternion.Euler(0, 0, angle),
            0.2f
        );
        SpawnObjectNow(Effect, transform.position, 2);
        audioManager.PlaySE(audioClip);

    }
    void Add(int count)
    {
        RecoveryCount += count;
    }
}
