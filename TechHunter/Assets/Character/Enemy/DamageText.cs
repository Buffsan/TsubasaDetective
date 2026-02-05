using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI DamageCountText;
    [SerializeField] float MoveSpeed =1;

    [SerializeField] Color NomalColor;
    [SerializeField] Color CriticulColor;

    public bool Criticul = false;
    bool one = false;
public float lifeTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        transform.position = new Vector2(transform.position.x,transform.position.y+MoveSpeed);
        
    }
    
    public void OnEnable()
    {
        if (Criticul)
        {
            DamageCountText.color = CriticulColor;
            transform.localScale = new Vector2(1.5f, 1.5f);
        }
        else 
        {
            DamageCountText.color = NomalColor;
            transform.localScale = new Vector2(1, 1);
        }
        CancelInvoke();
        Invoke("ReturnToPool", lifeTime);
    }

    public void isChangeText(float value) 
    {
        DamageCountText.text = value.ToString("F1");
    }
    void ReturnToPool()
    {
        DamageTextPool.instance.Return(gameObject);
    }
}
