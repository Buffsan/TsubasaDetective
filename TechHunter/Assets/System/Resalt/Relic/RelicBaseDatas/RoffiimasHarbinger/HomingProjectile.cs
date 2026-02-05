using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class HomingProjectile : MonoBehaviour
{
    public GameObject TargetObject;
    [SerializeField] Rigidbody2D rb;
    public float HomingStartTime = 0.5f;
    [SerializeField] float NoneTarget_DestroyTime = 0.2f;
    [SerializeField] float Speed = 10;
    [SerializeField] GameObject SpawnAttack;
    [SerializeField] float SpawnAttack_destroyTime;
    [SerializeField] GameObject Effect;
    [SerializeField] ParticleSystem particleSystem;
    float Count = 0;
    float destroyCount = 0;
    bool attacked = false;

    private void FixedUpdate()
    {
        if (TargetObject)
        {
            
            Count += Time.fixedDeltaTime;
            if(rb.velocity.magnitude > 0.02)rb.velocity = rb.velocity * 0.8f;
            if (Count < HomingStartTime) return;

            Vector2 TargetDirection = DirectionMath(TargetObject);
            rb.velocity = TargetDirection * Speed;
        }
        else 
        {
            destroyCount += Time.fixedDeltaTime;
            if (destroyCount > NoneTarget_DestroyTime) 
            {
                Destroy(gameObject);
            }
        }
    }
    Vector2 DirectionMath(GameObject TgameObject)
    {
    
        Vector2 direction = TgameObject.transform.position - transform.position;
        direction = direction.normalized;
        return direction;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("EnemyArea")|| attacked) return;
        
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;
        attacked = true;
        var emission = particleSystem.emission;
        emission.rateOverTime = 0;
        particleSystem.gameObject.transform.SetParent(null);
        Destroy(particleSystem.gameObject, 2);
        SpawnObject(SpawnAttack,SpawnAttack_destroyTime);
        SpawnObject(Effect, 3);
        Destroy(gameObject);
        
    }
    void SpawnObject(GameObject spawnObject,float dellcount) 
    {
        GameObject CL_Attack = Instantiate(spawnObject, transform.position, Quaternion.identity);
        Destroy(CL_Attack, dellcount);
    }
    
}
