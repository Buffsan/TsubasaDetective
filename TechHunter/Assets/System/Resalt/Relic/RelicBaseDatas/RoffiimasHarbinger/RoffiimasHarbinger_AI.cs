using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoffiimasHarbinger_AI : RelicBase_AI
{
    [SerializeField] float AttackCount = 0;
    [SerializeField] float AttackTime = 1;
    [SerializeField] float SpawnAttackNumber = 10;
    [SerializeField] GameObject Bullet;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;

    public override void BaseAction()
    {
        if (!playerController.SaveSkillBase) return;
        //0.2‚¾‚Á‚½ê‡‚Í‚P
        float speedMultiplier = 0.2f / Mathf.Max(0.05f, playerController.NowSkillMove);
        AttackCount += Time.fixedDeltaTime * speedMultiplier;
        if (AttackCount > AttackTime) 
        {

            AttackCount = 0;

            List<GameObject> enemys = new List<GameObject>();
            
            foreach (var allenemy in ALL_System.systemManager.AllEnemy)
            {
                if (allenemy) enemys.Add(allenemy);
            }
            if (enemys.Count <= 0) return;
            for (int i = 0; i < SpawnAttackNumber; i++) 
            {
                
                
                
                GameObject TargetEnemy;
                TargetEnemy = enemys[Random.Range(0, enemys.Count)];
                
                GameObject CL_Bullet = Instantiate(Bullet, transform.position, Quaternion.identity);
                Rigidbody2D rb = CL_Bullet.GetComponent<Rigidbody2D>();
                HomingProjectile homingProjectile = CL_Bullet.GetComponent<HomingProjectile>();
                homingProjectile.TargetObject = TargetEnemy;
                homingProjectile.HomingStartTime = Random.Range(0.3f,1f);
               
                Vector2 RandomShot = new Vector2(Random.Range(-1f,1f), Random.Range(-1f,1f));
                rb.velocity = RandomShot.normalized *20;
            }
                
        }
    }
}
