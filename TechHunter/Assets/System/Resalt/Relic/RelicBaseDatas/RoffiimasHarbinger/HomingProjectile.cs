using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if (rb != null && rb.velocity.sqrMagnitude > 0.0004f) rb.velocity *= 0.8f;
            if (Count < HomingStartTime) return;

            Vector2 TargetDirection = DirectionMath(TargetObject);
            if (rb != null) rb.velocity = TargetDirection * Speed;
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
        if (!other.gameObject.CompareTag("EnemyArea") || attacked) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;
        attacked = true;
        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = 0;
            particleSystem.transform.SetParent(null, true);
            Destroy(particleSystem.gameObject, 2f);
        }
        SpawnObject(SpawnAttack, SpawnAttack_destroyTime);
        SpawnObject(Effect, 3);
        Destroy(gameObject);

    }
    void SpawnObject(GameObject spawnObject, float dellcount)
    {
        if (spawnObject == null) return;
        GameObject CL_Attack = Instantiate(spawnObject, transform.position, Quaternion.identity);
        Destroy(CL_Attack, dellcount);
    }

}
