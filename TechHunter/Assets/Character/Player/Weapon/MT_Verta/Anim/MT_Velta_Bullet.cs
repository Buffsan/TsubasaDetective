using System.Collections;
using UnityEngine;

public class MT_Velta_Bullet : MonoBehaviour
{

    [SerializeField] float BulletSpeed = 4;
    [SerializeField] Rigidbody2D rb;

    [SerializeField] bool PlayerUse = true;

    float count = 0;
    float AttackCount = 0;
    [SerializeField] float MAX_TIME = 5;
    [SerializeField] float BULLET_RANGE = 3;

    [SerializeField]ParticleSystem particleSystem;
    [SerializeField] ParticleSystem particleSystem1;

    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject Big_Velta_Attack;

    [SerializeField] float endTime = 8;
    float endCount = 0;

    PlayerController playerController => PlayerController.Instance;
    Vector2 Direction;
    float Distanse;
    int Phase = 0;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void FixedUpdate()
    {
        if (Phase == 0)
        {
            if (MAX_TIME > count)
                count += Time.deltaTime;
            if (PlayerUse)
            {
                Direction = playerController.cursorController.MousePos - transform.position;
                Distanse = Vector2.Distance(playerController.cursorController.MousePos, transform.position);
            }
            else 
            {
                Direction = playerController.transform.position - transform.position;
                Distanse = Vector2.Distance(playerController.transform.position, transform.position);
                endCount += Time.fixedDeltaTime;
                if (endCount > endTime) 
                {
                    count = 0;
                    Phase = 1;
                    rb.velocity = Vector2.zero;
                }
            }
            
            rb.velocity = Direction.normalized * (BulletSpeed + count);
            if (Distanse < BULLET_RANGE)
            {
                count = 0;
                Phase = 1;
                rb.velocity = Vector2.zero;
            }

        }
        if (Phase == 1)
        {

            particleSystem.emissionRate = 0;
            particleSystem1.emissionRate = 0;
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(NeedleSpawnLing((float)i / 4, i));
            }
            Destroy(gameObject, 2);
            Phase = 2;
        }
        

    }

    private IEnumerator NeedleSpawnLing(float delayTime,int number)
    {
        //Debug.Log("コルーチンが開始されました。今から " + delayTime + " 秒待ちます...");
        // ここで指定した秒数だけ待機する
        yield return new WaitForSeconds(delayTime);
        GameObject CL_BIG = Instantiate(Big_Velta_Attack, transform.position, Quaternion.identity);
        GameObject CL_Attack = Instantiate(AttackArea, transform.position, Quaternion.identity);
        ALL_SystemManager.Instance.camera_Controller.Shake(0.1f,0.2f);
        if (number == 0)
        {
            CL_BIG.transform.Rotate(0, 0, -30);
        }
        else if (number == 1)
        {
            CL_BIG.transform.Rotate(0, 0, 30);
        }
        Destroy(CL_Attack, 0.2f);
        Destroy(CL_BIG, 2);

    }
}
