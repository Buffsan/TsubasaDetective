using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Connect selection UI to shop item information and dialogue text tokens.</summary>
public class DialogueShopSelection : MonoBehaviour
{
    [SerializeField] List<DialogueShopItemData> items = new List<DialogueShopItemData>();
    [SerializeField] TextMeshProUGUI itemNameText, priceText, descriptionText;
    [SerializeField] Image iconImage;
    DialogueShopItemData selectedItem;

    public DialogueShopItemData SelectedItem { get { return selectedItem; } }

    public void SelectByIndex(int index)
    {
        if (index >= 0 && index < items.Count) SetSelectedItem(items[index]);
    }

    public void SetSelectedItem(DialogueShopItemData item)
    {
        selectedItem = item;
        Refresh();
    }

    void OnEnable() { Refresh(); }

    public void Refresh()
    {
        if (itemNameText != null) itemNameText.text = selectedItem != null ? selectedItem.itemName : "";
        if (priceText != null) priceText.text = selectedItem != null ? selectedItem.price.ToString("N0") : "";
        if (descriptionText != null) descriptionText.text = selectedItem != null ? selectedItem.description : "";
        if (iconImage != null)
        {
            iconImage.sprite = selectedItem != null ? selectedItem.icon : null;
            iconImage.enabled = selectedItem != null && selectedItem.icon != null;
        }
        if (DialogueUIController.Instance != null)
            DialogueUIController.Instance.SetSelectedShopItem(selectedItem);
    }
}
