using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_HeavySniperHand : MonoBehaviour
{

    PlayerController controller =>PlayerController.Instance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        Vector2 Distance = controller.transform.position - transform.position;
        transform.up = Distance;
        transform.Rotate(0, 0, 90);

    }
}
