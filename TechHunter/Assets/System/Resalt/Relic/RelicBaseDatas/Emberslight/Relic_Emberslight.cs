using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_Emberslight : RelicBase_AI
{

    [SerializeField] GameObject Light;
    private void OnEnable()
    {
        GlobalEvents.OnPlayerDamage += OnDamage;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnPlayerDamage -= OnDamage;
    }
    public void OnDamage(float damage, bool Invincible) 
    { 
        GameObject CL_Light = Instantiate(Light,transform.position,Quaternion.identity);
        Destroy(CL_Light,0.3f);
    }
}
