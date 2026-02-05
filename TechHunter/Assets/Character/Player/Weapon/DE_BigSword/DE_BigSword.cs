using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DE_BigSword : MonoBehaviour
{

    public float AttackCount = 0;
    [SerializeField] GameObject Attack;
    [SerializeField] GameObject AttackPoint;

    [SerializeField] GameObject Effect;

    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    [SerializeField] AudioClip Clip0;
    [SerializeField] AudioClip Clip1;

    AudioManager audioManager=> AudioManager.instance; 

    bool AttackBool = false;
    bool audioBool = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1);
        if (AttackCount > 1 && !audioBool) 
        { 
        audioBool = true;
            audioManager.PlaySE(Clip0);
        }
        if (AttackCount > 1.2)
        {

            if (!AttackBool)
            {
                ALL_System.camera_Controller.Shake(0.2f, 0.2f);
                AttackBool = true;
                audioManager.PlaySE(Clip1);
                GameObject CL_Attack = Instantiate(Attack, AttackPoint.transform.position, Quaternion.identity);
                GameObject CL_effect = Instantiate(Effect, transform.position, Quaternion.identity);
                Destroy(CL_Attack, 0.15f);
                Destroy(CL_effect, 1);
            }
        }
        if (AttackCount > 1.9) 
        {
            Destroy(gameObject);
        }

            AttackCount += Time.deltaTime;
    }
}
