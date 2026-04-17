using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketItemUI : MonoBehaviour
{
   public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Image iconImage;

    private MergeableItemData itemData;
    private MarketManager marketManager;

     public void Setup(MergeableItemData data, MarketManager manager)
    {
        itemData = data;
        marketManager = manager;

        if (itemNameText != null)
            itemNameText.text = data.ItemName;

        if (priceText != null)
            priceText.text = data.BuyPrice.ToString();

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

       // Optionally set the icon image if you have one in your data
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }

    
    private void OnBuyClicked()
    {
        if (marketManager == null || itemData == null) return;

        marketManager.TryBuyItem(itemData);
    }
}
