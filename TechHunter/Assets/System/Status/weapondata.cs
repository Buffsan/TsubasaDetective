using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/CreateWeaponData")]
public class weapondata : ScriptableObject
{
    public enum AttackType 
    { 
        Slash,
        Pierce,
        Blunt,
        Magic,
        None
    }
    public AttackType attackType = AttackType.None;
    
    public float Attack;
    public float AttackSpeedTime;
    public float StaggerPower;
    public float ConfusionPower;
    public float KnockBackPower;

    public int MaxStack = 0;
    
    public AudioClip HitAudio;
}
