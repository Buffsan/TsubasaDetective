using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_PD_DeadlyVenomEatAway_Staff : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject SpawnObject;
    [SerializeField] GameObject SpawnObject1;
    [SerializeField] float AttackSpeed = 12;
    [SerializeField] float AttackSpawnTime = 1;
    [SerializeField] float AttackFinishTime = 3;
    [SerializeField] float AttackSpawnTicer = 0;
    [SerializeField] float AttackFinishTicer =0;
    [SerializeField] float AttackDestroyTime = 2;
    PlayerController controller => PlayerController.Instance;

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (AttackFinishTime >= AttackFinishTicer) 
        { 
        
            AttackFinishTicer += Time.fixedDeltaTime;
            AttackSpawnTicer += Time.fixedDeltaTime;
            if (AttackSpawnTicer >= AttackSpawnTime) 
            {
                AttackSpawnTicer = 0;

                GameObject Spawn = SpawnObject;
                float random = Random.RandomRange(0,10);
                if (random == 9) 
                {
                    Spawn = SpawnObject1;
                }

                GameObject CL_SpawnAttack = Instantiate(Spawn, transform.position,Quaternion.identity);
                Rigidbody2D rb = CL_SpawnAttack.GetComponent<Rigidbody2D>();
                controller.isCursorDirection();
                CL_SpawnAttack.transform.up = controller.CursorDirection.normalized;
                rb.velocity = controller.CursorDirection.normalized * AttackSpeed;
                Destroy(CL_SpawnAttack, AttackDestroyTime);
            
            }
        
        }
    }
}
