using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    [SerializeField] GameObject Smoke;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 7; i++) 
        {
            GameObject CL_Smoke = Instantiate(Smoke, transform.position, Quaternion.identity);
            Rigidbody2D rb = CL_Smoke.GetComponent<Rigidbody2D>();
            Vector2 vero = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            rb.velocity = vero.normalized * 1.5f;
            Destroy(CL_Smoke, 0.7f);
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
