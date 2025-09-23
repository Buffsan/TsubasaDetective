using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public weapondata weapondata;
    public SkillCardData skillcarddata;
    public int AttackLevel = 0;


     float Level_AttackBONUS_multiplier = 50;
     float Level_AttackBONUS = 0.5f;
    PlayerController controller => PlayerController.Instance;
    public AudioManager audioManager => AudioManager.instance;

    public bool AgeinAttack = false;

    List<GameObject> EnemyData = new List<GameObject>();
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("EnemyArea")) 
        { 
        IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && controller != null) 
            {
                bool Attack = true;
                foreach (GameObject againenemy in EnemyData) 
                {
                    if (againenemy == other.gameObject) 
                    { 
                    Attack = false;
                    }
                }

                if (Attack || AgeinAttack)
                {
                    if (AgeinAttack) { EnemyData.Add(other.gameObject); }
                    
                    audioManager.isPlaySE(weapondata.HitAudio);
                    float CriticalRandom = Random.Range(1f, 100f);
                    if (controller.AddCritical+controller.Critical <= CriticalRandom)
                    {
                        damageable.Damage(AttackMathf(), weapondata.StaggerPower, weapondata.ConfusionPower,false);
                    }
                    else 
                    {
                        Debug.Log("CriticalII");
                        damageable.Damage(AttackMathf()*3, weapondata.StaggerPower, weapondata.ConfusionPower,true);
                    }
                        controller.HitEffectSpawn(other.transform.position);
                }
            }
        }
    }
    float AttackMathf() 
    {
        float AttackAll;
        float Level_BONUS_Damage = 0;
        float Level_BONUS_Damage_multiply = 0;

        if (skillcarddata != null) 
        {
            foreach (var skillinfo in controller.skillINFO) 
            {
                if (skillinfo.skillDATA == skillcarddata) 
                {
                    AttackLevel = skillinfo.SkillLevel -1;

                    Level_BONUS_Damage = Level_AttackBONUS * AttackLevel;
                    Level_BONUS_Damage_multiply = Level_AttackBONUS_multiplier * AttackLevel;

                    break;
                }
            }
        }

        AttackAll = ((weapondata.Attack + controller.ATK + Level_BONUS_Damage) * ((100 + controller.AddATK + Level_BONUS_Damage_multiply) / 100) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff;
       return AttackAll;
    }

}
