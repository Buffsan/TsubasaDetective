using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.ParticleSystem;

public class DestroyPlayerAttack : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] int AttackType = 0;
    [SerializeField] float DestroyTime = 0;
    [SerializeField] GameObject SpawnAttack;
    [SerializeField] float SpawnAttackDestroyTime = 0.1f;
    [SerializeField] float AttackSpeed = 6;
    [SerializeField] GameObject MainObject;
    [SerializeField] GameObject SubObject;
    [SerializeField] float SubObjectDestroyTime = 5;
    [SerializeField] List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    bool touch = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("EnemyArea")) 
        {

            charaDamage charaDamage = other.GetComponent<charaDamage>();
            
            if (charaDamage != null && !touch)
            {
                touch = true;
                if (MainObject)
                {
                    if (SubObject) 
                    {
                        SubObject.transform.parent = null;
                        if (particleSystems.Count > 0) 
                        {
                            foreach (ParticleSystem p in particleSystems) 
                            {
                                var emission = p.emission;
                                emission.rateOverTime = 0f;
                            }
                        }
                        Destroy(SubObject, SubObjectDestroyTime);
                    }
                }
                else 
                {
                    MainObject = gameObject;
                }
                
                if (AttackType == 1) 
                { 
                
                    PlayerController playerController = PlayerController.Instance;
                    GameObject CL_Attack = Instantiate(SpawnAttack, transform.position, Quaternion.identity);
                    CL_Attack.transform.up = playerController.CursorDirection.normalized;
                    Destroy(CL_Attack, SpawnAttackDestroyTime);
                   
                }
                Destroy(MainObject, DestroyTime);
            }
        }
    }
}
