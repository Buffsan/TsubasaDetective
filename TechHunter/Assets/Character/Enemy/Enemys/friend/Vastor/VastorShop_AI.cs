using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VastorShop_AI : MonoBehaviour
{

    [SerializeField] Animator animator;
    [SerializeField] Animator kaiwaAnim;
    [SerializeField] ShopManager shopManager;
    public bool Shop = false;
    bool find = false;
    [SerializeField] GameObject NeckSpot;
    [SerializeField] GameObject Necks;
    [SerializeField] GameObject Head;
    [SerializeField] Rigidbody2D Headrb;
    PlayerController playerController => PlayerController.Instance;
    SystemManager systemManager => SystemManager.Instance;
    private void Start()
    {
        kaiwaAnim = systemManager.shopWindowAnimator;
    }

    Vector2 targetPos = Vector2.zero;
    [SerializeField] Vector2 UpPos = new Vector2(0, 3);
    public List<GameObject> Neck = new List<GameObject>();
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        Shop = false;
        foreach (var shop in shopManager.shopControllers)
        {
            if (shop.PlayerFind)
            {
                Shop = true;
            }
        }
        NeckPosMove();
        if (Shop)
        {
            find = true;
            animator.Play("見る");
            Necks.SetActive(true);
            Head.SetActive(true);
            kaiwaAnim.Play("テスト動作出現");
            targetPos = (Vector2)playerController.transform.position + UpPos;
            Headrb.velocity = (targetPos - (Vector2)Head.transform.position) * 3;
            HeadChange();
        }
        else
        {
            animator.Play("待機");
            float Distance = Vector2.Distance(NeckSpot.transform.position, Head.transform.position);

            if(find)kaiwaAnim.Play("テスト動作");
            if (Distance < 1)
            {
                Necks.SetActive(false);
                Head.SetActive(false);
            }
            Headrb.velocity = ((Vector2)NeckSpot.transform.position - (Vector2)Head.transform.position) * 6;
        }
    }
    void NeckPosMove() 
    {
        int count = Neck.Count;

        // 始点～終点を count+1 分割し、その間に配置
        for (int i = 0; i < count; i++)
        {
            float t = (float)(i + 1) / (count + 1); // ★ここが重要
            Vector3 pos = Vector3.Lerp(NeckSpot.transform.position, Head.transform.position, t);

            Transform tr = Neck[i].transform;
            tr.position = pos;

            Vector3 dir = (Head.transform.position - pos).normalized;
            if (dir != Vector3.zero)
            {
                tr.transform.up = dir;
            }
        }
    }

    void HeadChange() 
    {
        float trne = 1;
        if (transform.position.x > Head.transform.position.x)
        {
            trne = -1;
        }
        else 
        {
            trne = 1;
        }
        Head.transform.localScale = new Vector3(trne,1,1);
    }
}
