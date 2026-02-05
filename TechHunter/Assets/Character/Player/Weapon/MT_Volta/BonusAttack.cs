using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BonusAttack : MonoBehaviour
{
    [SerializeField] float AttackDellTime = 1f;
    [SerializeField] GameObject AttackObject;
    [SerializeField] GameObject Effect;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("EnemyArea") && !AttackObject) return;
        
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        GameObject CL_Attack = Instantiate(AttackObject,other.transform.position,Quaternion.identity);
        EffectPool.Instance.Get(Effect, other.transform.position, Quaternion.identity, 9, other.transform);
        CL_Attack.transform.parent = other.transform;
        Destroy(CL_Attack,AttackDellTime);
        
    }
}
