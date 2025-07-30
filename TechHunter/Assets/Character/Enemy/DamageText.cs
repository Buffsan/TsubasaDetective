using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI DamageCountText;
    [SerializeField] float MoveSpeed =1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.position = new Vector2(transform.position.x,transform.position.y+MoveSpeed);
    }
    public void isChangeText(float value) 
    {
        DamageCountText.text = value.ToString();
    }
}
