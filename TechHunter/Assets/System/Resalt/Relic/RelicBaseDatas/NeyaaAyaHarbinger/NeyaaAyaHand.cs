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
    [SerializeField] float MAXSPEED = 30;
    [SerializeField] float ReturnTimeout = 5f;
    float returnElapsed;
    PlayerController playerController => PlayerController.Instance;
    [SerializeField] float WaitTime = 0.5f;
    float waitCount = 0;
    // Start is called before the first frame update
    [SerializeField] int Phase = 0;
    SystemManager systemManager => SystemManager.Instance;
    void Start()
    {

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Phase == 0)
        {
            //TargetPos = playerController.cursorController.MousePos;
            if (systemManager == null || systemManager.AllEnemy == null || systemManager.AllEnemy.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            // 破棄済み要素を選んだ場合にも安全に対象を決定する。
            GameObject enemy = null;
            int start = Random.Range(0, systemManager.AllEnemy.Count);
            for (int i = 0; i < systemManager.AllEnemy.Count; i++)
            {
                GameObject candidate = systemManager.AllEnemy[(start + i) % systemManager.AllEnemy.Count];
                if (candidate != null) { enemy = candidate; break; }
            }
            if (enemy == null) { Destroy(gameObject); return; }

            TargetPos = enemy.transform.position;
            Vector2 dir = TargetPos - (Vector2)Head.transform.position;
            Head.transform.up = dir;

            Phase++;
        }

        if (Phase == 1)
        {
            Vector2 dir = TargetPos - (Vector2)Head.transform.position;
            float targetDistance = dir.magnitude;

            resultUpSpeed = Mathf.Min(MAXSPEED, resultUpSpeed + upSpeed * Time.fixedDeltaTime);



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
            Vector2 direction = (Vector2)transform.position - (Vector2)Head.transform.position;
            float distance = direction.magnitude;
            returnElapsed += Time.fixedDeltaTime;
            if (distance <= 0.3f || returnElapsed >= ReturnTimeout)
            {
                rb.velocity = Vector2.zero;
                Destroy(gameObject);
                return;
            }
            // 到達直前に減速し、行き過ぎと微小速度による取り残しを防止。
            rb.velocity = direction.normalized * Mathf.Min(MAXSPEED, distance / Time.fixedDeltaTime);
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
