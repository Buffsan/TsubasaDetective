using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    public Vector3 MousePos;
    public Vector3 MouseScreenPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    
    private void FixedUpdate()
    {
        MouseScreenPos = Input.mousePosition;   
        MousePos = Camera.main.ScreenToWorldPoint(MouseScreenPos);
        MousePos.z = 0;

        transform.position = MousePos;
    }
}
