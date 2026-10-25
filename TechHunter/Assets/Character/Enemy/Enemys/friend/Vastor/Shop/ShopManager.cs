using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Optional dialogue-based shopping")]
    [Tooltip("Uses the existing ShopItems and shop price formula. Without a DialogueCanvas, the old instant-buy input still works.")]
    [SerializeField] bool useDialogueForPurchases = true;
    [SerializeField] DialogueCharacterId shopkeeperId = DialogueCharacterId.Vastor;
    [SerializeField] TextMeshProUGUI focusedShopNameText;
    [SerializeField] TextMeshProUGUI focusedShopPriceText;
    [SerializeField] TextMeshProUGUI focusedShopDescriptionText;

    ShopController focusedShop;
    DialogueShopItemData previewItem;
    DialogueData shopConversation;
    bool shopConversationActive;
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
        if (shopConversationActive && DialogueUIController.Instance != null &&
            DialogueUIController.Instance.IsPlaying)
        {
            // Cancelling a stage unload must never spend coins.
            shopConversationActive = false;
            DialogueUIController.Instance.Finish();
        }
    }

    void OnDestroy()
    {
        if (shopConversation != null) Destroy(shopConversation);
        if (previewItem != null) Destroy(previewItem);
    }
    public void OnDecide(float damage)
    {
        if (shopConversationActive) return;

        ShopController selected = focusedShop != null && focusedShop.PlayerFind
            ? focusedShop : null;
        if (selected == null)
        {
            for (int i = 0; i < shopControllers.Count; i++)
            {
                if (shopControllers[i] != null && shopControllers[i].PlayerFind)
                {
                    selected = shopControllers[i];
                    break;
                }
            }
        }
        if (selected == null) return;

        if (useDialogueForPurchases && DialogueUIController.Instance != null)
        {
            StartShopConversation(selected);
            return;
        }
        // Preserves the old instant-purchase behaviour in scenes without dialogue UI.
        BuyItem(selected);
    }

    public static string ItemDisplayName(ItemData item)
    {
        switch (item)
        {
            case ItemData.SkillPage: return "スキルカード";
            case ItemData.NomalSkillPage_Range: return "遠距離スキルカード";
            case ItemData.NomalSkillPage_Meele: return "近距離スキルカード";
            case ItemData.StatasPage: return "ステータスカード";
            case ItemData.RelicPage: return "遺物";
            default: return item.ToString();
        }
    }

    public void SetFocusedShop(ShopController shop)
    {
        focusedShop = shop;
        RefreshShopPreview(shop);
    }

    public void ClearFocusedShop(ShopController shop)
    {
        if (focusedShop != shop) return;
        focusedShop = null;
        foreach (ShopController candidate in shopControllers)
        {
            if (candidate != null && candidate != shop && candidate.PlayerFind)
            {
                focusedShop = candidate;
                break;
            }
        }
        RefreshShopPreview(focusedShop);
    }

    public void RefreshShopPreview(ShopController shop)
    {
        if (shop == null || shop.shopItemDatas == null)
        {
            if (focusedShopNameText != null) focusedShopNameText.text = "";
            if (focusedShopPriceText != null) focusedShopPriceText.text = "";
            if (focusedShopDescriptionText != null) focusedShopDescriptionText.text = "";
            return;
        }

        if (previewItem == null)
        {
            previewItem = ScriptableObject.CreateInstance<DialogueShopItemData>();
            previewItem.hideFlags = HideFlags.DontSave;
        }

        // Read the original data and computed current price; do not copy the
        // old serialized ShopItems array into a second permanent item database.
        ShopItemDatas original = shop.shopItemDatas;
        previewItem.itemId = original.itemData.ToString();
        previewItem.itemName = string.IsNullOrWhiteSpace(original.ItemName)
            ? ItemDisplayName(original.itemData) : original.ItemName;
        previewItem.price = Mathf.RoundToInt(shop.AllValue);
        previewItem.description = original.Explanation;
        previewItem.icon = original.ItemImage;

        if (focusedShopNameText != null) focusedShopNameText.text = previewItem.itemName;
        if (focusedShopPriceText != null) focusedShopPriceText.text = previewItem.price.ToString("N0");
        if (focusedShopDescriptionText != null) focusedShopDescriptionText.text = previewItem.description;

        if (DialogueUIController.Instance != null && !shopConversationActive)
            DialogueUIController.Instance.SetSelectedShopItem(previewItem);
    }

    void StartShopConversation(ShopController shop)
    {
        DialogueUIController dialogueUI = DialogueUIController.Instance;
        if (dialogueUI == null || dialogueUI.IsPlaying || shopConversationActive) return;
        if (playerController == null || playerController.movetype == PlayerController.MoveType.Wait ||
            notBuyCount < notBuyTime) return;

        focusedShop = shop;
        RefreshShopPreview(shop);
        bool canAfford = playerController.AllCoins >= Mathf.RoundToInt(shop.AllValue);
        DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
        dialogue.hideFlags = HideFlags.DontSave;
        dialogue.participants.Add(shopkeeperId);

        dialogue.nodes.Add(new DialogueData.Node
        {
            nodeId = 0, speakerId = shopkeeperId, expression = DialogueExpression.Normal,
            text = "{itemName}ですね。価格は{itemPrice}コインです。\\n{itemDescription}"
        });
        var choice = new DialogueData.Node
        {
            nodeId = 10, type = DialogueData.NodeType.Choice,
            speakerId = shopkeeperId, text = "いかがなさいますか？"
        };
        choice.choices.Add(new DialogueData.Choice { label = "購入する", targetNodeId = 20 });
        choice.choices.Add(new DialogueData.Choice { label = "やめる", targetNodeId = 30 });
        dialogue.nodes.Add(choice);
        dialogue.nodes.Add(new DialogueData.Node
        {
            nodeId = 20, speakerId = shopkeeperId, expression = DialogueExpression.Happy,
            text = canAfford ? "ありがとうございます。お品物をご用意します。" :
                "申し訳ありません。コインが足りないようです。",
            nextNodeId = 40
        });
        dialogue.nodes.Add(new DialogueData.Node
        {
            nodeId = 30, speakerId = shopkeeperId, text = "かしこまりました。",
            nextNodeId = 40
        });
        dialogue.nodes.Add(new DialogueData.Node
        {
            nodeId = 40, speakerId = shopkeeperId, text = "ほかにもぜひご覧ください。"
        });

        bool wantsPurchase = false;
        shopConversation = dialogue;
        dialogueUI.SetSelectedShopItem(previewItem);
        shopConversationActive = true;
        bool started = dialogueUI.Play(dialogue,
            () =>
            {
                // Dialogue closes and unlocks controls before the purchased card UI opens.
                shopConversationActive = false;
                if (shopConversation == dialogue) shopConversation = null;
                Destroy(dialogue);
                if (this != null && isActiveAndEnabled && wantsPurchase &&
                    shop != null && shop.PlayerFind)
                    BuyItem(shop);
            },
            (nodeId, optionIndex) =>
            {
                if (nodeId == 10)
                    wantsPurchase = optionIndex == 0 && canAfford;
            });

        if (!started)
        {
            shopConversationActive = false;
            shopConversation = null;
            Destroy(dialogue);
        }
    }
    public void BuyItem(ShopController shopControllers) 
    {
        if (playerController.movetype == PlayerController.MoveType.Wait || notBuyCount < notBuyTime) return;
        int price = Mathf.RoundToInt(shopControllers.AllValue);
        if (playerController.AllCoins >= price)
        {
            playerController.AllCoins -= price;
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
    [Tooltip("Optional UI name. Empty uses the built-in name for ItemData.")]
    public string ItemName;

}
