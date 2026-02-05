using System.Collections.Generic;
using UnityEngine;

public class Skill_GS_KIRIKAESI_AI : MonoBehaviour
{
    [SerializeField] BuffData buffdata;
    [SerializeField] GameObject Kiri;
    [SerializeField] int MAX_SPAWN_KIRI_COUNT = 3;
    [SerializeField] float MAX_KIRI_SPEED = 0.5f;
    [SerializeField] float MIN_KIRI_SPEED = 0.5f;
    [SerializeField] List<GS_KIRISAKI_KIRI> KIRIs = new List<GS_KIRISAKI_KIRI> ();
    
    float AttackCcount = 0;
    public bool Counter = true; 
    int Phase = 0;
    PlayerController playerController => PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {

        playerController.playerBuff.SpawnBuff(buffdata);
        
        for (int i = 0; i < MAX_SPAWN_KIRI_COUNT ; i++)
        {

            SpawnKIRI();

        }
    }
    private void FixedUpdate()
    {
        AttackCcount += Time.fixedDeltaTime;
        if (Phase == 0)
        {
            if (AttackCcount > buffdata.BuffTime - 0.5f)
            {
                Counter = false;
                Phase++;
                ALL_SystemManager.Instance.camera_Controller.Shake(0.2f,0.1f * KIRIs.Count * 0.4f);
                foreach (var kiri in KIRIs) 
                { 
                    kiri.KIRISAKI_finish();
                }
                Destroy(gameObject, 1);
            }
        }
        if (Phase ==1) 
        {
            
            Phase++;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
       EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
        if (!enemyAttack) return;
            if (!Counter) return;
            for (int i = 0; i < MAX_SPAWN_KIRI_COUNT; i++) 
            {

                SpawnKIRI();
            
            }
        
    }
    void SpawnKIRI() 
    { 
        GameObject CL_kiri = Instantiate(Kiri,transform.position,Quaternion.identity);
        GS_KIRISAKI_KIRI kiri = CL_kiri.GetComponent<GS_KIRISAKI_KIRI>();
        KIRIs.Add(kiri);
        kiri.MainObject = gameObject;
        Rigidbody2D rb = CL_kiri.GetComponent<Rigidbody2D>();
        Vector2 randomDirection = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        rb.velocity = randomDirection.normalized * Random.Range(MIN_KIRI_SPEED,MAX_KIRI_SPEED);
    }
}
