using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DK_Halberd : MonoBehaviour
{

    [SerializeField] GameObject ICE_Slash;
    [SerializeField] GameObject AttackPoint;
    [SerializeField] GameObject AttackArea;

    [SerializeField] AudioClip Clip1;
    [SerializeField] AudioClip Clip2;
    float AttackCount = 0;
    bool Attack = false;
    bool Attack1 = false;

    bool OneMore = false;
    Vector2 SaveDirection = Vector2.zero;

    AudioManager audioManager = AudioManager.instance;
    PlayerController playerController => PlayerController.Instance;

    // Start is called before the first frame update
    void Start()
    {
        

        gameObject.transform.SetParent(playerController.MainAnimBody.transform);
        audioManager.isPlaySE(Clip2);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localScale = new Vector3(1, 1, 1);
        if (AttackCount > 0.4)
        {
            if (!Attack) 
            {
                
                GameObject CL_ICEslash = Instantiate(ICE_Slash, transform.position, Quaternion.identity);
                Rigidbody2D rb = CL_ICEslash.GetComponent<Rigidbody2D>();
                CL_ICEslash.transform.up = SaveDirection;
                CL_ICEslash.transform.Rotate(0, 0, 90);
                
                

                    rb.velocity = SaveDirection * 10;
                
                Destroy(CL_ICEslash, 1);

                
                Attack = true;

            }
        }
        if (AttackCount > 0.37)
        {

            if (!Attack1) 
            {
                audioManager.isPlaySE(Clip1);
                GameObject CL_AttackArea = Instantiate(AttackArea, AttackPoint.transform.position, Quaternion.identity); 
                Destroy(CL_AttackArea, 0.1f);
                Attack1 = true;
            }
        }
        if (!Attack) 
        {

            
        
        }
        if (!OneMore) 
        { 
        SaveDirection = playerController.CursorDirection.normalized;
            transform.up = playerController.CursorDirection.normalized;
        }

            AttackCount += Time.deltaTime;
        
    }
}
