using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    PlayerController playerController => PlayerController.Instance;
    public ALL_SystemManager ALL_System => ALL_SystemManager.Instance;
    AudioManager audioManager => AudioManager.instance;

    [SerializeField] float notBuyTime  = 0.2f;
    float notBuyCount = 0;

    public float UpValue = 0.2f;
    public float UpStageValue = 0.5f;
    [SerializeField] GameObject shopControllerObject;

    public List<GameObject> ShopPoint = new List<GameObject>();
    public List<ShopItemDatas> ShopItems = new List<ShopItemDatas>();
    public List<ShopController> shopControllers = new List<ShopController>();
    public List<ShopItemOrderList> shopItemOrderLists = new List<ShopItemOrderList>();
    public List<AudioClip> audioClips = new List<AudioClip>();
    public enum ItemData 
    { 
    
        SkillPage,
        NomalSkillPage_Range,
        NomalSkillPage_Meele,
        StatasPage,
        RelicPage,
    
    }
    void Start() 
    { 
        SpawnShopArea();
    }
    private void FixedUpdate()
    {
        if (playerController.movetype == PlayerController.MoveType.Wait)
        {
            notBuyCount = 0;
            return;
        }
      
        if (notBuyCount < notBuyTime)
        {
            notBuyCount += Time.fixedDeltaTime;
        }
        
    }
    public void SpawnShopArea() 
    {
        for(int i =0; i < ShopPoint.Count;i++)
        {
            GameObject CL_ShopArea = Instantiate(shopControllerObject, ShopPoint[i].transform.position, Quaternion.identity);
            CL_ShopArea.transform.parent = transform;
            ShopController shopController = CL_ShopArea.GetComponent<ShopController>();
            shopController.shopManager = this;
            switch (shopItemOrderLists[i].getItem) 
            {
                case ItemData.SkillPage:
                    shopController.shopItemDatas = ShopItems[0];
                    break;
                case ItemData.NomalSkillPage_Range:
                    shopController.shopItemDatas = ShopItems[3];
                    break;
                case ItemData.NomalSkillPage_Meele:
                    shopController.shopItemDatas = ShopItems[4];
                    break;
                case ItemData.StatasPage:
                    shopController.shopItemDatas = ShopItems[1];
                    break;
                case ItemData.RelicPage:
                    shopController.shopItemDatas = ShopItems[2];
                    break;
            }
            shopController.ChangeUIs();
            shopControllers.Add(shopController);
        }
    }
    private void OnEnable()
    {
        GlobalEvents.OnDecide += OnDecide;
    }

    private void OnDisable()
    {
        // イベント購読を解除（忘れるとバグの原因）
        GlobalEvents.OnDecide -= OnDecide;
    }
    public void OnDecide(float damage)
    {
        for (int i = 0; i < shopControllers.Count; i++) 
        {
            if (shopControllers[i].PlayerFind) 
            {
                BuyItem(shopControllers[i]);
                break;
            }
        }
    }
    public void BuyItem(ShopController shopControllers) 
    {
        if (playerController.movetype == PlayerController.MoveType.Wait || notBuyCount < notBuyTime) return;
        if (playerController.AllCoins - shopControllers.AllValue > 0)
        {
            playerController.AllCoins -= (int)shopControllers.AllValue;
            shopControllers.BuyNumber++;
            audioManager.PlaySE(audioClips[0]);
            ItemChoiceBuy(shopControllers.shopItemDatas.itemData);
            shopControllers.ChangeUIs();
        }
        else 
        { 
        
        }
    }
    void ItemChoiceBuy(ItemData itemData) 
    {
        switch (itemData) 
        { 
                case ItemData.SkillPage:
                ALL_System.skillController.isAll_StartSkillPageChoice(2);
                break;
                case ItemData.NomalSkillPage_Range:
                ALL_System.skillController.isAll_StartSkillPageChoice(1);
                break;
                case ItemData.NomalSkillPage_Meele:
                ALL_System.skillController.isAll_StartSkillPageChoice(0);
                break;                
                case ItemData.StatasPage:
                ALL_System.skillController.isStartPageChoice();
                break;
                case ItemData.RelicPage:
                ALL_System.skillController.All_StartRelicPageSpawn();
                break;
        }
    }
  
}
[Serializable]
public class ShopItemOrderList 
{ 
    public Vector2 spawnpos = Vector2.zero;
    public ShopManager.ItemData getItem = ShopManager.ItemData.SkillPage;

    bool randomItem = false;
    public List<ShopManager.ItemData> randomItemData = new List<ShopManager.ItemData>();
}
[Serializable]
public class ShopItemDatas 
{

    public ShopManager.ItemData itemData = ShopManager.ItemData.StatasPage;
    public int value = 0;
    public int BuyNumber = 1;
    public Sprite ItemImage;
    [TextArea]
    public string Explanation;

}
