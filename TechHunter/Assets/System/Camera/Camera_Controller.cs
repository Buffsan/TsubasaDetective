using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public Vector3 MoveCamera = new Vector3 (5, 5,-10);
    [SerializeField] GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y, MoveCamera.z);
        if (transform.position.x > MoveCamera.x)
        {
            transform.position = new Vector3(MoveCamera.x, transform.position.y, -10);
        }
        else if(transform.position.x < -MoveCamera.x)
        {
            transform.position = new Vector3(-MoveCamera.x, transform.position.y, -10);
        }
        if (transform.position.y > MoveCamera.y)
        {
            transform.position = new Vector3(transform.position.x,MoveCamera.y, -10);
        }
        else if (transform.position.y < -MoveCamera.y)
        {
            transform.position = new Vector3(transform.position.x, -MoveCamera.y,-10);
        }
    }

    public void isChangeNumber(Vector2 Newpos)
    {
        MoveCamera = Newpos;
    }
}
