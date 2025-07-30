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

    PlayerController playerController => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Direction = playerController.transform.position - transform.position;
        rb.velocity = Direction.normalized * Speed;
        
    }
}
