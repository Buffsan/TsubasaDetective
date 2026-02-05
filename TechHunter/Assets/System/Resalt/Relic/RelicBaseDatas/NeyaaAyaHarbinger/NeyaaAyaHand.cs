using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeyaaAyaHand : MonoBehaviour
{
    [SerializeField] GameObject Head;
    [SerializeField] SpriteRenderer HeadSprite;
    public List<GameObject> Neck = new List<GameObject>();
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float upSpeed = 1;
    [SerializeField] float resultUpSpeed = 0;
    [SerializeField] Vector2 TargetPos = Vector2.zero;
    [SerializeField] GameObject Attack;
    [SerializeField] Sprite newHand;
    PlayerController playerController => PlayerController.Instance;
    [SerializeField] float WaitTime = 0.5f;
    float waitCount = 0;
    // Start is called before the first frame update
    [SerializeField]int Phase = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Phase == 0)
        {
            TargetPos = playerController.cursorController.MousePos;

            Vector2 dir = TargetPos - (Vector2)Head.transform.position;
            Head.transform.up = dir;

            Phase++;
        }

        if (Phase == 1)
        {
            Vector2 dir = TargetPos - (Vector2)Head.transform.position;
            float targetDistance = dir.magnitude;

            resultUpSpeed += upSpeed * Time.fixedDeltaTime;
            rb.velocity = dir.normalized * resultUpSpeed;

            if (targetDistance < 0.4f)
            {
                AttackPool.Instance.Get(
                    Attack,
                    Head.transform.position,
                    Quaternion.identity,
                    0.1f
                );
  
                HeadSprite.sprite = newHand;
                rb.velocity = Vector2.zero;
                Phase++;
            }
        }

        if (Phase == 2)
        {
            waitCount += Time.fixedDeltaTime;
            if (waitCount > WaitTime) 
            {
                Phase++;
                waitCount = 0;
            }
        }
        if (Phase == 3) 
        {
            Vector2 Direction = (Vector2)transform.position - (Vector2)Head.transform.position;
            float targetDistanse = Vector2.Distance(transform.position, Head.transform.position);
            rb.velocity = Direction;
            if (targetDistanse < 0.1)
            {
                Destroy(gameObject);
            }
        }
        NeckPosMove();

    }
    void NeckPosMove()
    {
        int count = Neck.Count;

        // 始点～終点を count+1 分割し、その間に配置
        for (int i = 0; i < count; i++)
        {
            float t = (float)(i + 1) / (count + 1); // ★ここが重要
            Vector3 pos = Vector3.Lerp(transform.position, Head.transform.position, t);

            Transform tr = Neck[i].transform;
            tr.position = pos;

            Vector3 dir = (Head.transform.position - pos).normalized;
            if (dir != Vector3.zero)
            {
                tr.transform.up = dir;
            }
        }
    }
}
