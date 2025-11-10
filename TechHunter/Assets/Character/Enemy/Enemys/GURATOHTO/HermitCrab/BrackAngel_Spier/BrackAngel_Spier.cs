using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrackAngel_Spier : MonoBehaviour
{
    public Vector2 TargetPos;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float Speed = 2;
    [SerializeField] GameObject Spier;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] GameObject Effect;
    [SerializeField] List<AudioClip> audioClips = new List<AudioClip>();
    int AttackType = 0;

    AudioManager audioManager => AudioManager.instance;
    float Count =0;
    bool isSpawnSpier = false;
    bool isGoal = false;
    private void FixedUpdate()
    {
        if (TargetPos == null) return;
        if (isGoal)
        {
            if (!isSpawnSpier)
            {
                rb.velocity = Vector3.zero;
                isSpawnSpier = true;
                GameObject CL_Effect = Instantiate(Effect, transform.position, Quaternion.identity);
                    Destroy(CL_Effect, 3); particleSystem.emissionRate = 0;
                audioManager.isPlaySE(audioClips[0]);
                
            }
            else 
            {
                Count += Time.fixedDeltaTime;
                if (Count > 1)
                {
                    
                    
                    GameObject CL_Spier = Instantiate(Spier, transform.position, Quaternion.identity);



                    Destroy(gameObject);
                    Destroy(CL_Spier, 5);
                }
            }
            
        }
        else 
        { 
        Vector2 TargetDirection = TargetPos - (Vector2)transform.position;
        float Distance = Vector2.Distance(TargetPos, (Vector2)transform.position);
        rb.velocity = TargetDirection.normalized * Speed;
        if (Distance > 0.5) return;
            isGoal = true;
        }
        
    }
}
