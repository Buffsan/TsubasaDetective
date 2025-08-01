using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
   
    public EnemyBase enemyBase;
    public charadata charadata;

    public enum DellCondition 
    { 
    
        none,
        DirectionAttack_enum

    }
    public DellCondition dellCondition = DellCondition.none;

    public AudioManager audioManager => AudioManager.instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (dellCondition == DellCondition.DirectionAttack_enum && enemyBase.moveType != EnemyBase.MoveType.Attack) 
        { 
        DestroyArea();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerArea"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (enemyBase != null)
                {
                    damageable.Damage(enemyBase.ATK, 0, 0);
                }
                else if (charadata != null) 
                {
                    damageable.Damage(charadata.ATK, 0, 0);
                }

            }
            
        }
    }
    public void DestroyArea() 
    { 
    Destroy(gameObject);
    }
}
