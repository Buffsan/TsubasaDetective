using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OW_SeaBeastHunting : MonoBehaviour
{
    public float AttackCount = 0;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject portalLit;
    [SerializeField] GameObject AttackPoint;

    [SerializeField] GameObject Effect;

    [SerializeField] bool LitAttack = false;

    Vector2 SaveDirection = Vector2.zero;
    Vector2 SavePos = Vector2.zero;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    [SerializeField] AudioClip Clip0;
    [SerializeField] AudioClip Clip1;
    [SerializeField] AudioClip Clip2;

    public float baseDistance = 2f;   // オブジェクト間の基本距離
    public float randomOffset = 0.5f; // ランダムなズレの範囲

    AudioManager audioManager => AudioManager.instance;
    PlayerController playerController => PlayerController.Instance;

    bool AttackBool = false;
    bool audioBool = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (AttackCount > 1 && !audioBool)
        {
            audioBool = true;
            audioManager.isPlaySE(Clip0);

        }
        if (AttackCount > 1.2)
        {

            if (!AttackBool)
            {

                SaveDirection = playerController.CursorDirection.normalized;

                ALL_System.camera_Controller.Shake(0.2f, 0.4f);
                AttackBool = true;
                
                GameObject CL_Attack = Instantiate(Attack, AttackPoint.transform.position, Quaternion.identity);
                CL_Attack.transform.up = SaveDirection;
                GameObject CL_effect = Instantiate(Effect, transform.position, Quaternion.identity);
                CL_effect.transform.up = SaveDirection;
                Destroy(CL_Attack, 0.15f);
                Destroy(CL_effect, 1);

                SavePos = transform.position;
                if (LitAttack)
                {
                    for (int i = 0; i < 35; i++)
                    {audioManager.isPlaySE(Clip1);
                        StartCoroutine(PlaceObject(i, SavePos, playerController.CursorDirection.normalized));

                    }
                }
                else 
                {baseDistance *= 1.5f;
                    for (int i = 0; i < 5; i++)
                    {
                        
                        audioManager.isPlaySE(Clip2);
                        StartCoroutine(PlaceObject(i, SavePos, playerController.CursorDirection.normalized));

                    }
                }
                
            }
        }
        else 
        {

            transform.up = playerController.CursorDirection.normalized;

        }
        if (AttackCount > 2.5)
        {
            Destroy(gameObject);
        }

        AttackCount += Time.fixedDeltaTime;
    }

    private IEnumerator PlaceObject(float index ,Vector3 savepos ,Vector3 playerpos)
    {
        yield return new WaitForSeconds(index/30);

        float randomRotate = Random.Range(0, 360);

        // 距離に応じて前方に配置
        float distance = baseDistance * (index + 5);
        Vector3 basePos = savepos + playerpos * distance;

        // 位置にランダムなズレを追加（円形範囲内で）
        Vector2 randomCircle = Random.insideUnitCircle * randomOffset  * index;
        Vector3 finalPos = basePos + new Vector3(randomCircle.x, randomCircle.y, 0);

        // オブジェクトを設置
        GameObject CL_litSlash = Instantiate(portalLit, finalPos, Quaternion.identity);
        CL_litSlash.transform.Rotate(0,0,randomRotate);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea")) 
        {
            LitAttack = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EnemyArea"))
        {
            LitAttack = false;
        }
    }
}
