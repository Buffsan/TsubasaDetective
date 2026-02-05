using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic_CrownOfTheGreatBirdSlayer_AI : RelicBase_AI
{
    [SerializeField] GameObject Hane;
    [SerializeField] GameObject Slash;

    [SerializeField] float SpawnHaneDistance = 1;
    [SerializeField] float HaneSpeed = 9;
    [SerializeField] float SlashSpeed = 18;

    [SerializeField] int SlashAttackCount = 20;
    int SlashCount = 0;
    float totalDistance = 0;
    Vector2 prevPos;

    PlayerController playerController => PlayerController.Instance;

    private void Start()
    {
        prevPos = transform.position;
    }

    public override void BaseAction()
    {
        Vector2 currentPos = transform.position;
        totalDistance += Vector2.Distance(prevPos, currentPos);
        prevPos = currentPos;

        
            if (SlashAttackCount <= SlashCount) 
            {
                List<GameObject> enemys = new List<GameObject>();
                SlashCount = 0;
                foreach (var allenemy in ALL_SystemManager.Instance.systemManager.AllEnemy)
                {
                    if (allenemy) enemys.Add(allenemy);
                }
                if (enemys.Count <= 0) return;

                GameObject CL_Slash = Instantiate(Slash, transform.position, Quaternion.identity);
                Rigidbody2D rb = CL_Slash.GetComponent<Rigidbody2D>();

                Vector2 Direction = enemys[Random.Range(0, enemys.Count)].transform.position - transform.position;
                rb.velocity = Direction.normalized * SlashSpeed;
                CL_Slash.transform.up = Direction;
                Destroy(CL_Slash, 5);
            }
        

        if (totalDistance > SpawnHaneDistance) 
        {
            totalDistance = 0;
            List<GameObject> enemys = new List<GameObject>();

            foreach (var allenemy in ALL_SystemManager.Instance.systemManager.AllEnemy)
            {
                if (allenemy) enemys.Add(allenemy);
            }
            if (enemys.Count <= 0) return;
            SlashCount++;

            
            GameObject CL_Hane = Instantiate(Hane, transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Hane.GetComponent<Rigidbody2D>();
            
            Vector2 Direction = enemys[Random.Range(0, enemys.Count)].transform.position - transform.position;
            rb.velocity = Direction.normalized * HaneSpeed;
            CL_Hane.transform.up = Direction;
            Destroy( CL_Hane ,10);
        }
    }
}
