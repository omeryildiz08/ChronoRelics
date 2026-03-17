using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("Market Data")]
    public List<MergeableItemData> marketItems = new List<MergeableItemData>();

    [Header("UI References")]
    public Transform itemListContainer;
    public GameObject marketItemUIPrefab;
    public TextMeshProUGUI feedbackText;
    public GameObject marketPanel;



    void Start()
    {
        BuildMarketUI();
    }

    public void BuildMarketUI()
    {
        if(itemListContainer == null ||  marketItemUIPrefab == null)
        {
            Debug.LogWarning("Market UI references eksik!");
            return;
        }

        ClearMarketUI();

        foreach (var item in marketItems)
        {
            if(item == null) continue;
            if(item.Prefab == null) continue;
            if(item.BuyPrice <= 0) continue;

            GameObject uiObj = Instantiate(marketItemUIPrefab, itemListContainer);
            MarketItemUI itemUI = uiObj.GetComponent<MarketItemUI>();

             if (itemUI == null)
            {
                Debug.LogError("Market item UI prefabinda MarketItemUI yok.");
                continue;
            }

             itemUI.Setup(item, this);

        }
    }

    private void ClearMarketUI()
    {
        for (int i = itemListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemListContainer.GetChild(i).gameObject);
        }
    }
     public void TryBuyItem(MergeableItemData itemData)
    {
        if (itemData == null)
        {
            SetFeedback("Item verisi bulunamadi.");
            return;
        }

        if (SaveManager.Instance == null)
        {
            SetFeedback("SaveManager bulunamadi.");
            return;
        }

        if (!SaveManager.Instance.CanAfford(itemData.BuyPrice))
        {
            SetFeedback("Yetersiz Time Credit.");
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            SetFeedback("GridManager bulunamadi.");
            return;
        }

        Vector2Int? emptyPos = gridManager.GetFirstEmptyPosition();
        if (!emptyPos.HasValue)
        {
            SetFeedback("Base grid dolu.");
            return;
        }

        bool spent = SaveManager.Instance.SpendTimeCredits(itemData.BuyPrice);
        if (!spent)
        {
            SetFeedback("Satin alma basarisiz.");
            return;
        }

        SpawnObjectAt(itemData, emptyPos.Value, gridManager);
        SaveManager.Instance.SaveGame();
        SetFeedback($"{itemData.ItemName} satin alindi.");
    }

     private void SpawnObjectAt(MergeableItemData itemData, Vector2Int pos, GridManager gm)
    {
        if (gm.grid[pos.x, pos.y].TileView == null) return;

        Vector3 worldPos = gm.grid[pos.x, pos.y].TileView.GetWorldPosition();
        GameObject newObj = Instantiate(itemData.Prefab, worldPos, Quaternion.identity);
        MergeableObject mergeable = newObj.GetComponent<MergeableObject>();

        if (mergeable != null)
        {
            mergeable.CurrentGridPosition = pos;
            mergeable.InitializeObject();
        }
        else
        {
            Debug.LogError($"Prefab ({itemData.ItemID}) uzerinde MergeableObject yok.");
            Destroy(newObj);
        }
    }

      private void SetFeedback(string message)
    {
        Debug.Log($"[Market] {message}");

        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void ToggleMarket()
    {
        if (marketPanel != null)
        {
            bool isActive = marketPanel.activeSelf;
            marketPanel.SetActive(!isActive);
        }
    }

}