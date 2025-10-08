using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GS_CurseDriveStaff_Fire : MonoBehaviour
{
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject Effect;

    [SerializeField] GameObject Fire;

    float AttackTime = 0.65f;
    float AttackCount = 0;

    [SerializeField] AudioClip AudioClip;

    ALL_SystemManager ALL_SystemManager => ALL_SystemManager.Instance;
    AudioManager audioManager => AudioManager.instance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        AttackCount += Time.fixedDeltaTime;
        if (AttackCount > AttackTime) 
        {
            ALL_SystemManager.camera_Controller.Shake(0.1f,0.2f);

            audioManager.isPlaySE(AudioClip);
        GameObject CL_Effect = Instantiate(Effect,transform.position,Quaternion.identity);
            Destroy(CL_Effect,5);
            GameObject CL_Attack = Instantiate(Attack, transform.position, Quaternion.identity);
            Destroy(CL_Attack, 0.2f);

            for (int i = 0; i < 10; i++) 
            {
                Vector2 NowPos = (Vector2)transform.position + new Vector2(Random.RandomRange(-3f, 3f), Random.RandomRange(-3f, 3f));
                Instantiate(Fire, NowPos, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
