using UnityEngine;

// Display-only adapter. The actual purchase/inventory logic remains in your shop.
[CreateAssetMenu(fileName = "ShopItem", menuName = "TechHunter/Dialogue/Shop Item")]
public class DialogueShopItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    [Min(0)] public int price;
    [TextArea(2, 7)] public string description;
    public Sprite icon;
}
