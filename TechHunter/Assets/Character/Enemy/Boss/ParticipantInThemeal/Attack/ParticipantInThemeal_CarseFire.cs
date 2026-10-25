using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticipantInThemeal_CarseFire : MonoBehaviour
{

    [SerializeField] GameObject Effect;
    [SerializeField] GameObject Bomb;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed = 1;

    Vector2 Direction;

    EnemyBase attackOwner;

    public void SetOwner(EnemyBase owner)
    {
        attackOwner = owner;
    }

    Transform CurrentTarget
    {
        get
        {
            Boss_Controller boss = attackOwner as Boss_Controller;
            if (boss != null) return boss.CurrentTarget;
            if (attackOwner != null)
                return attackOwner.Player != null ? attackOwner.Player.transform : null;
            // 旧Prefabを単独で生成した場合のみ従来のプレイヤー追尾に戻す。
            PlayerController player = PlayerController.Instance;
            return player != null ? player.transform : null;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Transform target = CurrentTarget;
        if (target == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Direction = target.position - transform.position;
        rb.velocity = Direction.normalized * Speed;
        
    }
}
