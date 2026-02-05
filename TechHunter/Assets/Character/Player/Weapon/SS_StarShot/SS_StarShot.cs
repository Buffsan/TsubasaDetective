using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SS_StarShot : MonoBehaviour
{
    [SerializeField] GameObject Arrow;
    [SerializeField] AudioClip Clip;
    [SerializeField] GameObject AttackPont;

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
        if (AttackCount > 3)
        {
            Destroy(gameObject);
        }

        controller.isCursorDirection();
        transform.up = controller.CursorDirection;
        //transform.Rotate(0, 0, 90);

        AttackCount += Time.deltaTime;
        if (ArrowCount <= 4 && AttackCount > 0.3)
        {
            
            AttackCount = 0;
            controller.isCursorDirection();

            float angleStep =  ArrowCount *7;
            float angle = -angleStep*2;
            for (int i = 0; i < 5; i++)
            {//Debug.Log("“Š‚°‚é");

                GameObject bullet = Instantiate(Arrow,transform.position, Quaternion.identity);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                Destroy(bullet, 15 + ArrowCount);

                Vector2 bulletDirection = Quaternion.Euler(0, 0, angle) * controller.CursorDirection.normalized;
                rb.velocity = bulletDirection *   (4*((43-angleStep) / 43));
                bullet.transform.up = bulletDirection;
                angle += angleStep;

                manager.PlaySE(Clip);

            }
            ArrowCount++;

        }
    }
}
