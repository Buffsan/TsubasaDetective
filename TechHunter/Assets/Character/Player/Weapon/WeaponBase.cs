using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{

    public weapondata weapondata;

    [HideInInspector]public float Attack;
    [HideInInspector]public float AttackSpeedTime;
    [HideInInspector] public float StaggerPower;
    [HideInInspector] public float ConfusionPower;
    [HideInInspector] public float KnockBackPower;
    public enum AttackType
    {
        Slash,
        Pierce,
        Blunt,
        Magic,
        None
    }
    AttackType attackType;
    // Start is called before the first frame update
    void Start()
    {
        Attack = weapondata.Attack;
        AttackSpeedTime = weapondata.AttackSpeedTime;
        StaggerPower = weapondata.StaggerPower;
        ConfusionPower = weapondata.ConfusionPower;
        KnockBackPower = weapondata.KnockBackPower;


        attackType = (AttackType)weapondata.attackType;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
