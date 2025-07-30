using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortLayerSystem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    
    public Renderer renderer;
    int baseLayer = 0;
    int layerLange = 200;
    float worldRange = 10f;

    public int PlusLayer = 0;

    public 

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
        // ���ۂ̃��[���h���W���N�����v
        float clampedPositionY = Mathf.Clamp(transform.position.y, -worldRange, worldRange);

        // �N�����v�������W����Ƀ��C���[��ݒ�
        int sortingOrder = baseLayer + Mathf.RoundToInt(clampedPositionY / worldRange * layerLange);
        
        if (spriteRenderer != null) 
        { 
        spriteRenderer.sortingOrder = sortingOrder*-1 + PlusLayer * -1;
        }
        if (renderer != null) 
        {
            renderer.sortingOrder = sortingOrder + PlusLayer * -1;
        }
    }
}
