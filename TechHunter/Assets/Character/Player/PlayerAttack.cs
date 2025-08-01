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
                    damageable.Damage(AttackMathf(), weapondata.StaggerPower, weapondata.ConfusionPower);
                    controller.HitEffectSpawn(other.transform.position);
                }
            }
        }
    }
    float AttackMathf() 
    {
        float CriticalRandom = Random.Range(1f,100f);
        float AttackAll;
        if (controller.AddCritical <= CriticalRandom)
        {
             AttackAll = ((weapondata.Attack + controller.ATK) * (1 + controller.AddATK / 10) + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff;
        }
        else 
        {
            Debug.Log("CriticalII");
            AttackAll = (((weapondata.Attack + controller.ATK) * (1 + controller.AddATK / 10)  + controller.playerBuff.Attack_AllBuff) * controller.AddMultiplierATK * controller.playerBuff.MultiplyAttack_AllBuff) *3;
        }

        return AttackAll;
    }

}
