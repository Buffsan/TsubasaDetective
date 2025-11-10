using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine;

public class SB_HermitCrab_AI : EnemyMoveBase
{
    [SerializeField] GameObject BrackAngel;
    [SerializeField] float RandomAttackSpawn = 2f;
    [SerializeField] float WalkAttackTime = 0.5f;
    float WalkAttackCount = 0;

    [SerializeField] float WaitTime = 4;
    float WaitCount = 0;

    private IEnumerator myRunningCoroutine; // 実行中のコルーチンを保持するための変数
    public float duration = 1;
    public float arcHeight = 30;
    bool StandOk = false;
    Vector2 targetPos = Vector2.zero;
    [SerializeField] GameObject Denger_AttackArea;
    [SerializeField] GameObject Denger_Attackarea1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void EnemyStanPlay()
    {
        AttackFinish();
        attacktype = 0;
    }
    public override void EnemyConfusionPlay()
    {
        AttackFinish();
        attacktype = 0;
    }
    public override void EnemyMovePlay()
    {
        enemyBase.isPlayerLookBase();
        enemyBase.animator.SetInteger("Anim", 1);
        enemyBase.rb.velocity = enemyBase.PlayerDirection.normalized * enemyBase.SPEED;
        WalkAttackCount += Time.fixedDeltaTime;
        WaitCount += Time.fixedDeltaTime;
        if (WaitCount > WaitTime) 
        { 
            WaitCount = 0;
            attacktype = 1;
            enemyBase.moveType = EnemyBase.MoveType.Attack;
        }
        if (WalkAttackCount > WalkAttackTime) 
        { 
            Vector2 RandomPos = new Vector2 (Random.Range(-RandomAttackSpawn, RandomAttackSpawn), Random.Range(-RandomAttackSpawn, RandomAttackSpawn));
            RandomPos += (Vector2)transform.position;
            BrackAngel_Spawn(RandomPos);
            WalkAttackCount = 0;
        }
    }
    public override void EnemyAttackPlay()
    {
        AttackCount += Time.fixedDeltaTime;
        if (attacktype == 0)
        {
            if (A_Phase == 0)
            {
                enemyBase.animator.SetInteger("Anim", 2);
                if (AttackCount > 1)
                {
                    WaitCount = 0;
                    AttackNext();
                    enemyBase.rb.velocity = Vector2.zero;
                }
            }
            if (A_Phase == 1)
            {
                enemyBase.animator.SetInteger("Anim", 3);
                AttackNext();
                for (int i = 0; i < 30; i++)
                {
                    StartCoroutine(NeedleSpawnLing(i / 20, i, (Vector2)enemyBase.Player.transform.position));
                }

            }
            if (A_Phase == 2)
            {
                if (AttackCount > 2)
                {
                    enemyBase.isPlayerLookBase();
                    AttackFinish();
                    enemyBase.animator.SetInteger("Anim", 0);
                }
            }
        }
        if (attacktype == 1) 
        {
            if (A_Phase == 0)
            {
                enemyBase.animator.SetInteger("Anim", 2);
                if (AttackCount > 1)
                {
                    AttackNext();
                    targetPos = enemyBase.Player.transform.position;
                    enemyBase.rb.velocity = Vector2.zero;
                    SpawnObjectNow(Denger_AttackArea,enemyBase.Player.transform.position ,1);
                }
            }
            if (A_Phase == 1)
            {
                StartCoroutine(MoveInArc());
                enemyBase.animator.SetInteger("Anim", 3);
                AttackNext();
                

            }
            if (A_Phase == 2)
            {
                if (StandOk)
                {

                    StandOk = false;
                    enemyBase.isPlayerLookBase();
                    AttackFinish();
                    enemyBase.animator.SetInteger("Anim", 0);

                    for (int i = 0; i < 20; i++) 
                    {
                        Vector2 RandomPos = new Vector2(Random.Range(-RandomAttackSpawn/2, RandomAttackSpawn/2), Random.Range(-RandomAttackSpawn/2, RandomAttackSpawn/2));
                        RandomPos += (Vector2)transform.position;
                        BrackAngel_Spawn(RandomPos);
                    }
                    attacktype = 0;
                }
            }
        }
    }

    public GameObject BrackAngel_Spawn(Vector2 pos) 
    { 
    
        GameObject CL_Attack = Instantiate(BrackAngel,transform.position,Quaternion.identity);
        BrackAngel_Spier brackAngel_Spier = CL_Attack.GetComponent<BrackAngel_Spier>();
        brackAngel_Spier.TargetPos = pos;
        return CL_Attack;
    }
    private IEnumerator NeedleSpawnLing(float delayTime,int number,Vector2 Pos)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        Ougi(number, Pos);
    }

    void Ougi(int number ,Vector2 pos) 
    {
        enemyBase.AttackCount = 0;
        number++;

        float AttackDistance = number / 2.5f;

        // ランダムな角度を生成 (0 ~ 360度)
        float randomAngle = Random.Range(-40f, 40f);

        // ランダムな角度をラジアンに変換
        float radians = randomAngle * Mathf.Deg2Rad;

        // 極座標からデカルト座標へ変換
        float xOffset = AttackDistance * Mathf.Cos(radians);
        float yOffset = AttackDistance * Mathf.Sin(radians);
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        Vector2 newDirection = (pos - (Vector2)transform.position).normalized;

        // 新しい位置を計算
        //Vector2 newPosition = (Vector2)transform.position + newDirection*new Vector2(xOffset, yOffset);

        Vector2 rotatedDirection = rotation * newDirection; // SavePos方向を基準に角度を回転
        Vector2 newPosition = (Vector2)transform.position + rotatedDirection * AttackDistance; // 回転方向で新しい位置を計算

        BrackAngel_Spawn(newPosition);
    }

    private IEnumerator MoveInArc()
    {
        enemyBase.animator.Play("攻撃", 0, 0);
        myRunningCoroutine = MoveInArc();
        // 移動開始前のBodyTypeを保存しておく
        var originalBodyType = enemyBase.rb.bodyType;
        // 移動中はKinematicにして、物理演算の影響をなくす
        enemyBase.rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 startPos = enemyBase.transform.position;
        Vector2 endPos = targetPos;

        GameObject CL_AttackPPP = Instantiate(Denger_AttackArea, endPos, Quaternion.identity);

        Destroy(CL_AttackPPP, 1.5f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // 0から1の範囲に収める

            // XとYの直線的な移動を計算
            Vector2 linearPos = Vector2.Lerp(startPos, endPos, progress);

            // 高さを計算 (Sinカーブ)
            float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

            // Rigidbody2Dを使って位置を更新する
            // 2Dなので、Y座標に高さを加える
            enemyBase.rb.MovePosition(new Vector2(linearPos.x, linearPos.y + height));

            //yield return new WaitForFixedUpdate(); // 物理演算の更新タイミングで待つ
            yield return new WaitForEndOfFrame();
        }

        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = endPos;
        // BodyTypeを元に戻す
        enemyBase.rb.bodyType = originalBodyType;


        StandOk = true;
        // 念のため、最終的にきっちり目標地点に合わせる
        transform.position = targetPos;
        Debug.Log("目的地に到着！" + targetPos);
    }
}
