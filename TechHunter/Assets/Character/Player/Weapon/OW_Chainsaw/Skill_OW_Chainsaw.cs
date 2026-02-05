using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_OW_Chainsaw : MonoBehaviour
{
    PlayerController playerController => PlayerController.Instance;
    ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    AudioManager audioManager => AudioManager.instance;
    public GameObject AI_Object;
    [SerializeField] GameObject AttackArea;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] List<weapondata> weapondatas = new List<weapondata>();

    [SerializeField] AudioClip audioClip;
    [SerializeField] Animator animator;
    [SerializeField] List<float> AttackTime = new List<float>();
    [SerializeField]int AttackNumber = 10;
    [SerializeField]int AttackNumberCount = 0;
    float AttackCount = 0;
    [SerializeField]int AttackPhase = 0;
    int Phase = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioManager.PlaySE(audioClip);
    }
    void nextPhase() 
    { 
    Phase ++;
        AttackCount = 0;
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.up = playerController.CursorDirection.normalized;
        AttackCount += Time.fixedDeltaTime;
        if (Phase == 0) 
        {
            if (AttackCount > 0.5) 
            { 
            nextPhase();
                animator.Play("UŒ‚ŠJŽn");
            }
        }
        if (Phase == 1) 
        {

            if (AttackCount > AttackTime[AttackPhase]) 
            {
                ALL_System.camera_Controller.Shake(0.05f,0.1f);
                AttackNumberCount++;
                GameObject CL_AttackArea = Instantiate(AttackArea,AttackPoint.transform.position,Quaternion.identity);
                PlayerAttack playerAttack = CL_AttackArea.GetComponent<PlayerAttack>();
                playerAttack.weapondata = weapondatas[AttackPhase];
                
                CL_AttackArea.transform.up = playerController.CursorDirection.normalized;
                Destroy(CL_AttackArea, 0.1f);
                AttackCount = 0;
                if (AttackNumberCount > AttackNumber && AttackPhase < AttackTime.Count - 1)
                {
                    
                    AttackNumberCount = 0;
                    AttackPhase++;
                }
            }
            if (!AI_Object && AttackPhase !=0) 
            {
                nextPhase();
                animator.Play("UŒ‚I—¹");
            }
        }
        if (Phase == 2) 
        {
            if (AttackCount > 2) 
            {
                Destroy(gameObject);
            }
        }

        

    }
}
