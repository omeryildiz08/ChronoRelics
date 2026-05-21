using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    [Header("Market Data")]
    public List<MergeableItemData> marketItems = new List<MergeableItemData>();

    [Header("UI References")]
    public Transform itemListContainer;
    public GameObject marketItemUIPrefab;
    public TextMeshProUGUI feedbackText;
    public GameObject marketPanel;

    [Header("Sell State")]
    public MergeableObject selectedObject;

    [Header("Drag & Drop Sell UI")]
    public RectTransform sellZoneRect; 
    public GameObject sellZoneVisual; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BuildMarketUI();
    }

    public void BuildMarketUI()
    {
        if (itemListContainer == null || marketItemUIPrefab == null)
        {
            Debug.LogWarning("Market UI references eksik!");
            return;
        }

        ClearMarketUI();

        foreach (MergeableItemData item in marketItems)
        {
            if (item == null) continue;
            if (item.Prefab == null) continue;
            if (item.BuyPrice <= 0) continue;

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
        if (itemListContainer == null) return;

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

        bool spent = SaveManager.Instance.SpendTimeCredits(itemData.BuyPrice, false);
        if (!spent)
        {
            SetFeedback("Satin alma basarisiz.");
            return;
        }

        SpawnObjectAt(itemData, emptyPos.Value, gridManager);
        SaveManager.Instance.SaveGame();
        gridManager.NotifyBaseStateChanged();
        SetFeedback($"{itemData.ItemName} satin alindi.");
    }

    public void SelectObjectForSelling(MergeableObject targetObject)
    {
        if (targetObject == null)
        {
            selectedObject = null;
            SetFeedback("Secili obje temizlendi.");
            return;
        }

        selectedObject = targetObject;

        string itemName = targetObject.ItemData != null
            ? targetObject.ItemData.ItemName
            : "Bilinmeyen Obje";

        SetFeedback($"Secili obje: {itemName}");
    }

    public void TrySellSelectedItem()
    {
        if (selectedObject == null)
        {
            SetFeedback("Satmak icin base'den bir obje sec.");
            return;
        }

        TrySellObject(selectedObject);
    }

    public void TrySellObject(MergeableObject targetObject)
    {
        if (targetObject == null)
        {
            SetFeedback("Satilacak obje bulunamadi.");
            return;
        }

        MergeableItemData itemData = targetObject.ItemData;
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

        if (itemData.SellPrice <= 0)
        {
            SetFeedback("Bu obje satilamaz.");
            return;
        }

        if (targetObject.IsInactiveAnomalyItem)
        {
            SetFeedback("Anomali objeleri satilamaz.");
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            SetFeedback("GridManager bulunamadi.");
            return;
        }

        Vector2Int targetPos = targetObject.CurrentGridPosition;
        if (gridManager.IsTileLocked(targetPos))
        {
            SetFeedback("Kilitli tile uzerindeki obje satilamaz.");
            return;
        }

        bool removed = gridManager.RemoveObject(targetObject);
        if (!removed)
        {
            SetFeedback("Obje satilamadi.");
            return;
        }

        SaveManager.Instance.AddTimeCredits(itemData.SellPrice, false);
        SaveManager.Instance.SaveGame();
        selectedObject = null;
        SetFeedback($"{itemData.ItemName} satildi. +{itemData.SellPrice} Time Credit");
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
        if (marketPanel == null) return;

        bool isActive = marketPanel.activeSelf;
        marketPanel.SetActive(!isActive);
    }

    public void SetSellZoneActive(bool isActive)
    {
        if(sellZoneVisual != null)
        {
            sellZoneVisual.SetActive(isActive);
        }
    }
}
