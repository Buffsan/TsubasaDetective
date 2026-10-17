using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Puppeteer_Needle : MonoBehaviour
{
    
    [SerializeField] AudioClip Aclip;
    [SerializeField] AudioClip Bclip;
    [SerializeField] AudioClip StarClip;
    [SerializeField] float CountLimit = 3;

    // 0: 通常生成 / 1: 傀儡子専用プール
    public int AttackType = 0;
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

        if (Count > CountLimit && !A) 
        { 
            A = true;
            A_AudioPlay();
        }
    }

    // 再利用時のみ呼び出す。従来生成では初期化方式を変更しない。
    public void ResetForPool()
    {
        if (AttackType != 1) return;
        Count = 0;
        A = false;
        B = false;
        foreach (Animator animator in GetComponentsInChildren<Animator>(true))
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void A_AudioPlay() 
    {
        audioManager.PlaySE(Aclip);
        //Debug.Log("A");
    }
    public void B_AudioPlay()
    {
        audioManager.PlaySE(Bclip);
        //Debug.Log("B");
    }
}
