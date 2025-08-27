using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public weapondata weapondata;

    PlayerController controller => PlayerController.Instance;
    public AudioManager audioManager => AudioManager.instance;

    public bool AgeinAttack = false;

    List<GameObject> EnemyData = new List<GameObject>();
    // Start is called before the first frame update
    
    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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
                        Debug.Log("Critical！！");
                        damageable.Damage(AttackMathf()*3, weapondata.StaggerPower, weapondata.ConfusionPower,true);
                    }
                        controller.HitEffectSpawn(other.transform.position);
                }
            }
        }
    }
    float AttackMathf() 
    {
        //float CriticalRandom = Random.Range(1f,100f);
        float AttackAll;
        AttackAll = ((weapondata.Attack + controller.ATK) * ((100 + controller.AddATK) / 100) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff;

        /*
        if (controller.AddCritical <= CriticalRandom)
        {
             AttackAll = ((weapondata.Attack + controller.ATK) * ((100 + controller.AddATK)/100) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff;
        }
        else 
        {
            Debug.Log("Critical！！");
            AttackAll = (((weapondata.Attack + controller.ATK) * ((100 + controller.AddATK) / 100) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff) *3;
        }*/

        return AttackAll;
    }

}
