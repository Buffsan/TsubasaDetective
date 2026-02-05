using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_ActaWave : MonoBehaviour
{

    [SerializeField] float UpSizeSpeed = 1.0f;
    // Start is called before the first frame update
   
    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.localScale = transform.localScale + (Vector3.one * UpSizeSpeed * Time.fixedDeltaTime);
    }
}
