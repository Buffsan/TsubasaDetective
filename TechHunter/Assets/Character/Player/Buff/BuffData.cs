using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CreateBuffData")]
public class BuffData : ScriptableObject
{
    public bool Invincible=false;

    public string BuffNAME;

    public float BuffTime=1;
    
    public float Regen;

    public float SPEED;
    public float MultiplySPEED;

    public float ATK;
    public float MultiplyATK;

    public float ATKDF;
    public float MultiplyATKDF;



}
