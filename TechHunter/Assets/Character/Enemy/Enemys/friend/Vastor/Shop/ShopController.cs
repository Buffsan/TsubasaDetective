using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public ShopManager shopManager;
    [SerializeField] TextMeshProUGUI valueText;
    [SerializeField] SpriteRenderer ItemRender;
    [SerializeField] Animator cardAnimator;
    public ShopItemDatas shopItemDatas;

    public float AllValue = 0;
    public float BuyNumber = 0;
    public bool PlayerFind = false;

    

    public void ChangeUIs() 
    {
        AllValue = shopItemDatas.value * (1 + (shopManager.UpValue * BuyNumber) + (shopManager.UpStageValue * shopManager.ALL_System.systemManager.stageNumber));
      
        valueText.text = AllValue.ToString("F0");
        ItemRender.sprite = shopItemDatas.ItemImage;
    }
   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArea")) 
        { 
            PlayerFind = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerArea"))
        {
            PlayerFind = false;
        }
    }
}
