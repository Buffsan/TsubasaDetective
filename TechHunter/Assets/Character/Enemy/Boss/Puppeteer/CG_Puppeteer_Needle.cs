using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Puppeteer_Needle : MonoBehaviour
{
    
    [SerializeField] AudioClip Aclip;
    [SerializeField] AudioClip Bclip;
    [SerializeField] AudioClip StarClip;

    public EnemyAttack enemyAttack;
    AudioManager audioManager => AudioManager.instance;

    bool A = false;
    bool B = false;

    float Count = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Count += Time.deltaTime;
        if (!B) 
        {
            B = true;
            B_AudioPlay();
        }

        if (Count > 3 && !A) 
        { 
            A = true;
            A_AudioPlay();
        }
    }

    public void A_AudioPlay() 
    {
        audioManager.isPlaySE(Aclip);
        //Debug.Log("A");
    }
    public void B_AudioPlay()
    {
        audioManager.isPlaySE(Bclip);
        //Debug.Log("B");
    }
}
