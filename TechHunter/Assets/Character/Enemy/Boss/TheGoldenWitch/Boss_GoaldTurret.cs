using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_GoaldTurret : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] float rotateSpeed = 1;

    GameObject saveArrow;
    int Phase = 0;
    public override void EnemyConfusionPlay()
    {
        base.EnemyConfusionPlay();
    }
    public override void EnemyStanPlay()
    {
        base.EnemyStanPlay();
    }
    public override void EnemyAttackPlay()
    {
        AttackCount += Time.deltaTime;
        if (A_Phase == 0) 
        {
            saveArrow = SpawnObjectNow_Input(Arrow, transform.position, 60);
            saveArrow.transform.parent = transform;
            AttackNext();
        }
        if (A_Phase == 1) 
        {
            if(saveArrow)saveArrow.transform.Rotate(0, 0, rotateSpeed);
            if (AttackCount > 65)
            {
                enemyBase.charaDamage.Damage(1000000,1,1,false);
                AttackNext();
            }
        }
    }

    void shotBullet()
    {
        //Vector2 ShotPos = enemyBase.Player.transform.position - enemyBase.AttackSpawnPoint.transform.position;

        GameObject CL_Arrow = Instantiate(Arrow, enemyBase.transform.position, Quaternion.identity);
        Rigidbody2D rb = CL_Arrow.GetComponent<Rigidbody2D>();
        rb.velocity = enemyBase.PlayerDirection.normalized * 1.8f;

        EnemyAttack enemyAttack = CL_Arrow.GetComponent<EnemyAttack>();
        enemyAttack.enemyBase = enemyBase;

        CL_Arrow.transform.up = enemyBase.PlayerDirection;
        CL_Arrow.transform.Rotate(0, 0, 90);

        Destroy(CL_Arrow, enemyBase.AttackDellTimer);

    }
    
}

