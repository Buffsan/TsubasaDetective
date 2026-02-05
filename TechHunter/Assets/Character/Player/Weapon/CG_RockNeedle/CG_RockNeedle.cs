using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CG_RockNeedle : MonoBehaviour
{

    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] GameObject AttackPont;
    [SerializeField] GameObject PlasAttackArea;
    [SerializeField] GameObject Arrow2;
    int Phase = 0;
    AudioManager manager => AudioManager.instance;

    int ArrowCount = 0;

    float AttackCount = 0;
    float AttackTime = 0.12f;

    PlayerController controller => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Phase == 0)
        {
            controller.isCursorDirection();
            transform.up = controller.CursorDirection;
            transform.Rotate(0, 0, 90);
            //transform.Rotate(0, 0, 90);

            AttackCount += Time.deltaTime;
            if (ArrowCount == 0)
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                List<GameObject> enemyall = new List<GameObject>();


                for (int i = 0; i < 5; i++)
                {
                    if (i >= enemies.Length)
                    {
                        GameObject MinEnemy;
                        float MinDistance = 100000;
                        foreach (GameObject enemy in enemies)
                        {
                            float Distance = Vector2.Distance(controller.transform.position, enemy.transform.position);
                            if (Distance < MinDistance)
                            {

                                bool passenemy = false;
                                foreach (GameObject enemyslist in enemyall)
                                {
                                    if (enemy == enemyslist)
                                    {
                                        passenemy = true;
                                    }
                                }
                                if (!passenemy)
                                {
                                    enemyall.Add(enemy);
                                    MinDistance = Distance;
                                    MinEnemy = enemy;
                                }

                            }
                        }
                    }

                }

                ArrowCount = 1;
                for (int i = 0; i < 20; i++)
                {

                    StartCoroutine(NeedleSpawnLing((float)i / 3, (float)i/4, i));

                }
                foreach (GameObject enemysPos in enemyall)
                {
                    GameObject CL_Arrow = Instantiate(Arrow, enemysPos.transform.position, Quaternion.identity);

                    GameObject CL_Bomb = Instantiate(PlasAttackArea, enemysPos.transform.position, Quaternion.identity);
                    //manager.PlaySE(Clip);
                    Destroy(CL_Arrow, 10);
                    Destroy(CL_Bomb, 0.2f);
                }

            }
            
            if (AttackCount > 7)
            {
                Phase++;
            }
        }
        if (Phase == 1) 
        { 
        Destroy(gameObject,15);
            Phase++;
        }

    }

    private IEnumerator NeedleSpawnLing(float delayTime, float Lange, int Plass)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < (10 + Plass); i++)
        {

            float angle = (360 / (10 + Plass)) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = transform.position.x + 2 * Lange * Mathf.Cos(angleRad);
            float newY = transform.position.y + 2 * Lange * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            SpawnRockNeedle(newPosition);

        }
    }
    public void SpawnRockNeedle(Vector2 pos)
    {
        GameObject CL_Rock = Instantiate(Arrow, pos, Quaternion.identity);
        Destroy(CL_Rock, 5);
        GameObject CL_Bomb = Instantiate(PlasAttackArea, pos, Quaternion.identity);
        Destroy(CL_Bomb, 0.2f);
    }

}
