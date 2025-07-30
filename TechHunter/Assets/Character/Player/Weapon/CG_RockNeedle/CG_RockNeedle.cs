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
        controller.isCursorDirection();
        transform.up = controller.CursorDirection;
        transform.Rotate(0, 0, 90);
        //transform.Rotate(0, 0, 90);

        AttackCount += Time.deltaTime;
        if (ArrowCount == 0)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            List<GameObject> enemyall = new List<GameObject>();

        
            for (int i = 0; i < 10; i++) 
            {
                if (i <= enemies.Length)
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
            foreach (GameObject enemysPos in enemyall)
            {
                GameObject CL_Arrow = Instantiate(Arrow, enemysPos.transform.position, Quaternion.identity);
                
                GameObject CL_Bomb = Instantiate(PlasAttackArea, enemysPos.transform.position, Quaternion.identity);
                //manager.isPlaySE(Clip);
                Destroy(CL_Arrow, 10);
                Destroy(CL_Bomb, 0.2f);
            }
        }
        if (AttackCount > 7)
        {
            Destroy(gameObject);
        }

    }

    private IEnumerator NeedleSpawnLing(float delayTime, float Lange, int Plass,Vector2 pos)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");

        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        for (int i = 0; i < 3 + Plass; i++)
        {

            float angle = (360 / 3 + Plass) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float newX = pos.x + 1 * Lange+1 * Mathf.Cos(angleRad);
            float newY = pos.y + 1 * Lange+1 * Mathf.Sin(angleRad);

            // 新しい位置にクローンを作成
            Vector2 newPosition = new Vector2(newX, newY);

            GameObject CL_Arrow = Instantiate(Arrow2, newPosition, Quaternion.identity);

        }
    }

}
