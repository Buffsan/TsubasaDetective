using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_KINGYO_ZIKIN : EnemyMoveBase
{
    [SerializeField] GameObject Arrow;
    [SerializeField] GameObject Effect;
    Vector2 PlayerDirSave;
    [SerializeField] int AttackPhase = 0;
    [SerializeField] float spreadAngle = 15;
    [SerializeField] GameObject Denger_AttackArea;
    [SerializeField] GameObject Denger_Attackarea1;
    [SerializeField] GameObject AttackArea;
    [SerializeField] float MaxRandomTime = 3;
    [SerializeField] float MinRandomTime = 1;
    [SerializeField] float MaxRandomTime1 = 2;
    [SerializeField] float MinRandomTime1 = 5;
    [SerializeField] GameObject SpawnEnemy;
    [SerializeField] float DashSpeed = 3;
    public GameObject Target;

    bool one = false;
    Vector2 Direction;
    bool trune = false;
    public float RandomTime;
    float AttackCount1 = 0;

    [SerializeField] float moveRabge = 0;
    [SerializeField] float MinmoveRange = 1f;
    [SerializeField] float MaxniveRange = 4f;
    [SerializeField] float DashRange = 0.8f;

    public float WaitCunt = 0;
    bool oneSpawn = false;

    SystemManager systemManager => SystemManager.Instance;

    private void Start()
    {
        if (!oneSpawn)
        {
            if (SpawnEnemy)
            {
                for (int i = 0; i < 4; i++)
                {
                    Debug.Log("A");
                    GameObject CL_Enemy = Instantiate(SpawnEnemy, transform.position, Quaternion.identity);
                    EnemyContoroller enemyContoroller = CL_Enemy.GetComponent<EnemyContoroller>();
                    SS_FairySpear_AI sS_FairySpear_AI = enemyContoroller.moveBase.gameObject.GetComponent<SS_FairySpear_AI>();
                    sS_FairySpear_AI.Target = gameObject;
                    systemManager.AllEnemy.Add(CL_Enemy);
                }

            }
            oneSpawn = true;
        }
    }
    public override void EnemyConfusionPlay()
    {
        A_Phase = 0;
        enemyBase.AttackCount = 0;
    }
    public override void EnemyStanPlay()
    {
        A_Phase = 0;
        enemyBase.AttackCount = 0;
    }
    public override void EnemyMovePlay()
    {
        if (!one)
        {
            one = true;
            moveRabge = Random.Range(MinmoveRange, MaxniveRange);
            RandomTime = Random.Range(MinmoveRange, MaxniveRange);
            int boolrandom = Random.Range(0, 2);
            if (boolrandom == 0)
            {
                trune = true;
            }
            else
            {
                trune = false;
            }
        }
        WaitCunt += Time.deltaTime;
        if (WaitCunt > RandomTime)
        {
            A_Phase = Random.Range(0, 2);
            WaitCunt = 0;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
            RandomTime = Random.Range(MinRandomTime1, MaxRandomTime1);
        }
        float angle;
        float offset;
        if (Target)
        {

            enemyBase.PlayerDistance = Vector2.Distance(Target.transform.position, enemyBase.gameObject.transform.position);
            enemyBase.animator.SetInteger("Anim", 1);
            enemyBase.PlayerDirection = Target.transform.position - enemyBase.transform.position;
            angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
            offset = 100f / (enemyBase.PlayerDistance + 1f) * moveRabge; // 距離が遠いほど小さく
        }
        else
        {

            enemyBase.PlayerDistance = Vector2.Distance(enemyBase.Player.transform.position, enemyBase.gameObject.transform.position);
            enemyBase.animator.SetInteger("Anim", 1);
            enemyBase.PlayerDirection = enemyBase.Player.transform.position - enemyBase.transform.position;
            angle = Mathf.Atan2(enemyBase.PlayerDirection.y, enemyBase.PlayerDirection.x) * Mathf.Rad2Deg;
            offset = 100f / (enemyBase.PlayerDistance + 1f) * moveRabge; // 距離が遠いほど小さく
        }


        if (trune)
        {
            angle += offset;
        }
        else
        {
            angle -= offset;
        }

        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        enemyBase.isPlayerLookBase();

        if (Target)
        {
            enemyBase.rb.velocity = direction * enemyBase.SPEED * 3;
        }
        else
        {
            enemyBase.rb.velocity = direction * enemyBase.SPEED;
        }

    }

    public override void EnemyAttackPlay()
    {
        enemyBase.AttackCount += Time.deltaTime;
        if (enemyBase.mode == EnemyBase.ModeType.M1)
        {

            
            enemyBase.AttackCount = 0;
            enemyBase.mode = EnemyBase.ModeType.M2;
            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.audioManager.PlaySE(enemyBase.AttackStayAudio);
        }
        if (enemyBase.mode == EnemyBase.ModeType.M2)
        {
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.isPlayerLookBase();
                PlayerDirSave = enemyBase.PlayerDirection;
                Fire(PlayerDirSave.normalized);
                enemyBase.animator.SetInteger("Anim", 2);
                enemyBase.AttackCount = 0;
                enemyBase.audioManager.PlaySE(enemyBase.AttackAudio);
                enemyBase.mode = EnemyBase.ModeType.M3;
                

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M3)
        {
            
            if (enemyBase.AttackCount > 1)
            {
                enemyBase.DamageBody.SetActive(false);
                //enemyBase.EnemyAnimBody.SetActive(false);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M4;                

            }
        }
        if (enemyBase.mode == EnemyBase.ModeType.M4)
        {

            if (enemyBase.AttackCount > 1)
            {
                Vector2 randomPos = new Vector2(Random.Range(-6,6), Random.Range(-6, 6));
                enemyBase.transform.position = (Vector2)enemyBase.playerController.transform.position + Vector2.one * randomPos;
                enemyBase.DamageBody.SetActive(true);
                enemyBase.EnemyAnimBody.SetActive(true);

                enemyBase.animator.SetInteger("Anim", 4);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M5;
            }
            
        }
        if (enemyBase.mode == EnemyBase.ModeType.M5)
        {

            enemyBase.rb.velocity = Vector2.zero;
            enemyBase.StanHP = enemyBase.charadata.StanHP;

            if (enemyBase.AttackCount > 1)
            {

                enemyBase.animator.SetInteger("Anim", 0);
                enemyBase.AttackCount = 0;
                enemyBase.mode = EnemyBase.ModeType.M1;
                enemyBase.moveType = EnemyBase.MoveType.Move;
                enemyBase.PlayerDistance = 1000;
            }
        }

    }
    public void Fire(Vector2 direction)
    {
        // 発射方向を正規化
        direction = direction.normalized;

        // 中心・左・右の3方向を作る
        ShootBullet(direction); // 中心
        ShootBullet(Rotate(direction, spreadAngle));  // 右
        ShootBullet(Rotate(direction, -spreadAngle)); // 左
    }

    private void ShootBullet(Vector2 dir)
    {
        GameObject bullet = Instantiate(Arrow, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = dir * 1;
        Destroy(bullet, 20);
    }

    // 方向ベクトルを角度で回転させる関数
    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
