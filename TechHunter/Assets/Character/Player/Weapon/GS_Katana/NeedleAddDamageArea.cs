using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleAddDamageArea : MonoBehaviour ,IDamageable
{

    [SerializeField] GameObject AttackArea;
    [SerializeField] weapondata weapon;
    [SerializeField] DK_Needle _Needle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Damage(float Attackvalue, float Stanvalue, float ConfusionPower,bool criticul)
    {
        _Needle.charaDamage.Damage(weapon.Attack,weapon.StaggerPower,weapon.ConfusionPower,false);
    }
    public void Death()
    { 
    }
}
