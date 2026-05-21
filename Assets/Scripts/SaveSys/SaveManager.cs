using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private const int CurrentSaveVersion = 1;

    [Header("Veri Tabani (Referanslar)")]
    public List<MergeableItemData> AllGameItems;

    private readonly Dictionary<string, MergeableItemData> itemLookup = new Dictionary<string, MergeableItemData>();
    private string saveFilePath;

    public List<string> CurrentInventory = new List<string>();
    public List<SavedObjectData> CurrentSavedObjects = new List<SavedObjectData>();
    public List<LockedTileSaveData> CurrentLockedTiles = new List<LockedTileSaveData>();
    public List<UnlockedSceneLockedTileSaveData> CurrentUnlockedSceneLockedTiles = new List<UnlockedSceneLockedTileSaveData>();
    public int CurrentTimeCredits = 0;
    public int CurrentChronoCharge = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        ValidateDatabase();
        RefreshItemLookup();
        LoadRuntimeStateFromDisk();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void ValidateDatabase()
    {
        if (AllGameItems == null)
        {
            return;
        }

        HashSet<string> ids = new HashSet<string>();
        for (int i = 0; i < AllGameItems.Count; i++)
        {
            MergeableItemData item = AllGameItems[i];
            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(item.ItemID))
            {
                Debug.LogError($"KRITIK: {item.name} objesinin ItemID'si bos.");
                continue;
            }

            if (!ids.Add(item.ItemID))
            {
                Debug.LogError($"KRITIK: Cakisan ID tespit edildi: {item.ItemID}");
            }
        }
    }

    public void AddRewardToInventory(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            return;
        }

        CurrentInventory.Add(itemID);
        Debug.Log($"Envantere eklendi: {itemID}");
        SaveGame();
    }

    public void AddTimeCredits(int amount)
    {
        AddTimeCredits(amount, true);
    }

    public void AddTimeCredits(int amount, bool saveImmediately)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentTimeCredits += amount;
        Debug.Log($"Zaman Kredisi eklendi: +{amount}. Toplam: {CurrentTimeCredits}");

        if (saveImmediately)
        {
            SaveGame();
        }
    }

    public bool CanAfford(int amount)
    {
        return amount > 0 && CurrentTimeCredits >= amount;
    }

    public bool SpendTimeCredits(int amount)
    {
        return SpendTimeCredits(amount, true);
    }

    public bool SpendTimeCredits(int amount, bool saveImmediately)
    {
        if (amount <= 0 || !CanAfford(amount))
        {
            return false;
        }

        CurrentTimeCredits -= amount;
        Debug.Log($"Zaman Kredisi harcandi: {amount}");

        if (saveImmediately)
        {
            SaveGame();
        }

        return true;
    }

    public void AddChronoCharge(int amount)
    {
        AddChronoCharge(amount, true);
    }

    public void AddChronoCharge(int amount, bool saveImmediately)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentChronoCharge += amount;
        Debug.Log($"Chrono Charge eklendi: +{amount}. Toplam: {CurrentChronoCharge}");

        if (saveImmediately)
        {
            SaveGame();
        }
    }

    public bool CanSpendChronoCharge(int amount)
    {
        return amount <= 0 || CurrentChronoCharge >= amount;
    }

    public bool SpendChronoCharge(int amount)
    {
        return SpendChronoCharge(amount, true);
    }

    public bool SpendChronoCharge(int amount, bool saveImmediately)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!CanSpendChronoCharge(amount))
        {
            return false;
        }

        CurrentChronoCharge -= amount;
        Debug.Log($"Chrono Charge harcandi: {amount}. Kalan: {CurrentChronoCharge}");

        if (saveImmediately)
        {
            SaveGame();
        }

        return true;
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (!IsBaseSceneActive())
        {
            SaveInventoryOnly();
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("GridManager bulunamadi, kaydetme islemi iptal edildi.");
            return;
        }

        RefreshItemLookup();

        GameSaveData data = new GameSaveData
        {
            SaveVersion = CurrentSaveVersion,
            InventoryItemIDs = new List<string>(CurrentInventory),
            TimeCredits = CurrentTimeCredits,
            ChronoCharge = CurrentChronoCharge,
            SavedObjects = CollectSavedObjects(gridManager),
            LockedTiles = CollectLockedTiles(gridManager),
            UnlockedSceneLockedTiles = CollectUnlockedSceneLockedTiles(gridManager)
        };

        CurrentSavedObjects = new List<SavedObjectData>(data.SavedObjects);
        CurrentLockedTiles = new List<LockedTileSaveData>(data.LockedTiles);
        CurrentUnlockedSceneLockedTiles = new List<UnlockedSceneLockedTileSaveData>(data.UnlockedSceneLockedTiles);
        WriteSaveData(data);
        DebugSavedObjects("SAVE", data.SavedObjects);
        Debug.Log($"Base durumu kaydedildi: {saveFilePath}");
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!TryReadSaveData(out GameSaveData data))
        {
            Debug.LogWarning("Kayit dosyasi bulunamadi: " + saveFilePath);
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            return;
        }

        RefreshItemLookup();
        CurrentInventory = data.InventoryItemIDs != null
            ? new List<string>(data.InventoryItemIDs)
            : new List<string>();
        CurrentTimeCredits = data.TimeCredits;
        CurrentChronoCharge = GetChronoChargeForLoad(data);
        CurrentLockedTiles = GetLockedTilesForLoad(data);
        CurrentUnlockedSceneLockedTiles = GetUnlockedSceneLockedTilesForLoad(data);
        CurrentSavedObjects = GetSavedObjectsForLoad(data);
        DebugSavedObjects("LOAD", CurrentSavedObjects);

        Debug.Log("Oyun yüklendi: " + saveFilePath);
        Debug.Log($"Oyun yuklendi. Envanterde {CurrentInventory.Count} esya var.");
    }

    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        CurrentInventory.Clear();
        CurrentSavedObjects.Clear();
        CurrentLockedTiles.Clear();
        CurrentUnlockedSceneLockedTiles.Clear();
        CurrentTimeCredits = 0;
        CurrentChronoCharge = 0;

        if (IsBaseSceneActive() && GridManager.Instance != null)
        {
            ClearCurrentScene(GridManager.Instance);
        }

        Debug.Log("Save dosyasi silindi. Temiz baslangic.");
    }

    private bool IsBaseSceneActive()
    {
        return SceneManager.GetActiveScene().name == "BaseScene";
    }

    private void ClearCurrentScene(GridManager gridManager)
    {
        MergeableObject[] sceneObjects = FindObjectsOfType<MergeableObject>();
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            if (sceneObjects[i] != null)
            {
                sceneObjects[i].gameObject.SetActive(false);
                Destroy(sceneObjects[i].gameObject);
            }
        }

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                ResetTileToSceneDefault(gridManager, new Vector2Int(x, y));
            }
        }
    }

    private void ResetTileToSceneDefault(GridManager gridManager, Vector2Int pos)
    {
        if (!gridManager.IsValidPosition(pos))
        {
            return;
        }

        GridTileData tileData = gridManager.grid[pos.x, pos.y];
        tileData.ObjectOnTile = null;

        GridTileView tileView = tileData.TileView;
        if (tileView == null)
        {
            tileData.isLocked = false;
            return;
        }

        bool shouldBeLocked = tileView.StartLocked;
        if (shouldBeLocked)
        {
            gridManager.LockTile(pos);
        }
        else
        {
            gridManager.UnlockTile(pos);
        }

        tileView.UpdateVisuals(shouldBeLocked);
    }

    private void SaveInventoryOnly()
    {
        GameSaveData data = ReadExistingOrNewSaveData();
        data.InventoryItemIDs = new List<string>(CurrentInventory);
        data.TimeCredits = CurrentTimeCredits;
        data.ChronoCharge = CurrentChronoCharge;
        WriteSaveData(data);

        Debug.Log($"Sadece envanter guncellendi (level modu). SavedObjects korunuyor: {data.SavedObjects.Count}");
    }

    private void LoadRuntimeStateFromDisk()
    {
        if (!TryReadSaveData(out GameSaveData data))
        {
            return;
        }

        CurrentInventory = data.InventoryItemIDs != null
            ? new List<string>(data.InventoryItemIDs)
            : new List<string>();
        CurrentSavedObjects = GetSavedObjectsForLoad(data);
        CurrentLockedTiles = GetLockedTilesForLoad(data);
        CurrentUnlockedSceneLockedTiles = GetUnlockedSceneLockedTilesForLoad(data);
        CurrentTimeCredits = data.TimeCredits;
        CurrentChronoCharge = GetChronoChargeForLoad(data);
    }

    public void RebuildBaseScene()
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            return;
        }

        RefreshItemLookup();
        ClearCurrentScene(gridManager);

        for (int i = 0; i < CurrentUnlockedSceneLockedTiles.Count; i++)
        {
            gridManager.UnlockTile(CurrentUnlockedSceneLockedTiles[i].GridPos);
        }

        for (int i = 0; i < CurrentLockedTiles.Count; i++)
        {
            gridManager.LockTile(CurrentLockedTiles[i].GridPos);
        }

        DebugSavedObjects("REBUILD", CurrentSavedObjects);
        for (int i = 0; i < CurrentSavedObjects.Count; i++)
        {
            SavedObjectData objectData = CurrentSavedObjects[i];
            MergeableItemData itemData = GetItemDataByID(objectData.ItemID);

            if (itemData == null)
            {
                Debug.LogWarning($"Kayitli obje veritabaninda bulunamadi: {objectData.ItemID}");
                continue;
            }

            Debug.Log($"[SaveManager:REBUILD-SPAWN] Item={objectData.ItemID} Pos={objectData.GridPos}");
            SpawnObjectAt(itemData, objectData.GridPos, gridManager);
        }

        gridManager.NotifyBaseStateChanged();
    }

    private List<SavedObjectData> CollectSavedObjects(GridManager gridManager)
    {
        Dictionary<Vector2Int, SavedObjectData> objectsByPosition = new Dictionary<Vector2Int, SavedObjectData>();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.TileView == null || tile.ObjectOnTile == null || tile.ObjectOnTile.ItemData == null)
                {
                    continue;
                }

                Vector2Int position = new Vector2Int(x, y);
                objectsByPosition[position] = new SavedObjectData
                {
                    GridPos = position,
                    ItemID = tile.ObjectOnTile.ItemData.ItemID
                };
            }
        }

        MergeableObject[] sceneObjects = FindObjectsOfType<MergeableObject>();

        for (int i = 0; i < sceneObjects.Length; i++)
        {
            MergeableObject mergeableObject = sceneObjects[i];
            if (mergeableObject == null || mergeableObject.ItemData == null)
            {
                continue;
            }

            Vector2Int position = ResolveObjectGridPosition(mergeableObject, gridManager);
            if (!gridManager.IsValidPosition(position))
            {
                continue;
            }

            if (gridManager.grid[position.x, position.y].TileView == null)
            {
                continue;
            }

            if (objectsByPosition.ContainsKey(position))
            {
                continue;
            }

            objectsByPosition[position] = new SavedObjectData
            {
                GridPos = position,
                ItemID = mergeableObject.ItemData.ItemID
            };
        }

        return new List<SavedObjectData>(objectsByPosition.Values);
    }

    private void DebugSavedObjects(string phase, List<SavedObjectData> savedObjects)
    {
        if (savedObjects == null)
        {
            Debug.Log($"[SaveManager:{phase}] SavedObjects null");
            return;
        }

        Debug.Log($"[SaveManager:{phase}] SavedObjects count={savedObjects.Count}");
        for (int i = 0; i < savedObjects.Count; i++)
        {
            SavedObjectData objectData = savedObjects[i];
            Debug.Log($"[SaveManager:{phase}] Item={objectData.ItemID} Pos={objectData.GridPos}");
        }
    }

    private Vector2Int ResolveObjectGridPosition(MergeableObject mergeableObject, GridManager gridManager)
    {
        Vector2Int currentGridPosition = mergeableObject.CurrentGridPosition;
        if (gridManager.IsValidPosition(currentGridPosition) &&
            gridManager.grid[currentGridPosition.x, currentGridPosition.y].TileView != null)
        {
            return currentGridPosition;
        }

        return gridManager.WorldToGridPosition(mergeableObject.transform.position);
    }

    private List<LockedTileSaveData> CollectLockedTiles(GridManager gridManager)
    {
        List<LockedTileSaveData> lockedTiles = new List<LockedTileSaveData>();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.TileView == null || !tile.isLocked)
                {
                    continue;
                }

                lockedTiles.Add(new LockedTileSaveData
                {
                    GridPos = new Vector2Int(x, y)
                });
            }
        }

        return lockedTiles;
    }

    private List<UnlockedSceneLockedTileSaveData> CollectUnlockedSceneLockedTiles(GridManager gridManager)
    {
        List<UnlockedSceneLockedTileSaveData> unlockedTiles = new List<UnlockedSceneLockedTileSaveData>();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.TileView == null || !tile.TileView.StartLocked || tile.isLocked)
                {
                    continue;
                }

                unlockedTiles.Add(new UnlockedSceneLockedTileSaveData
                {
                    GridPos = new Vector2Int(x, y)
                });
            }
        }

        return unlockedTiles;
    }

    private List<SavedObjectData> GetSavedObjectsForLoad(GameSaveData data)
    {
        if (data.SavedObjects != null && data.SavedObjects.Count > 0)
        {
            return data.SavedObjects;
        }

        List<SavedObjectData> legacyObjects = new List<SavedObjectData>();
        if (data.SavedTiles == null)
        {
            return legacyObjects;
        }

        for (int i = 0; i < data.SavedTiles.Count; i++)
        {
            TileSaveData tileData = data.SavedTiles[i];
            if (string.IsNullOrEmpty(tileData.ItemID))
            {
                continue;
            }

            legacyObjects.Add(new SavedObjectData
            {
                GridPos = tileData.GridPos,
                ItemID = tileData.ItemID
            });
        }

        return legacyObjects;
    }

    private List<LockedTileSaveData> GetLockedTilesForLoad(GameSaveData data)
    {
        if (data.LockedTiles != null && data.LockedTiles.Count > 0)
        {
            return data.LockedTiles;
        }

        List<LockedTileSaveData> legacyLockedTiles = new List<LockedTileSaveData>();
        if (data.SavedTiles == null)
        {
            return legacyLockedTiles;
        }

        for (int i = 0; i < data.SavedTiles.Count; i++)
        {
            TileSaveData tileData = data.SavedTiles[i];
            if (!tileData.IsLocked)
            {
                continue;
            }

            legacyLockedTiles.Add(new LockedTileSaveData
            {
                GridPos = tileData.GridPos
            });
        }

        return legacyLockedTiles;
    }

    private List<UnlockedSceneLockedTileSaveData> GetUnlockedSceneLockedTilesForLoad(GameSaveData data)
    {
        if (data.UnlockedSceneLockedTiles != null)
        {
            return data.UnlockedSceneLockedTiles;
        }

        return new List<UnlockedSceneLockedTileSaveData>();
    }

    private bool TryReadSaveData(out GameSaveData data)
    {
        data = null;

        if (!File.Exists(saveFilePath))
        {
            return false;
        }

        string json = File.ReadAllText(saveFilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        data = JsonUtility.FromJson<GameSaveData>(json);
        return data != null;
    }

    private GameSaveData ReadExistingOrNewSaveData()
    {
        if (TryReadSaveData(out GameSaveData data))
        {
            return data;
        }

        return new GameSaveData();
    }

    private void WriteSaveData(GameSaveData data)
    {
        data.SaveVersion = CurrentSaveVersion;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    private int GetChronoChargeForLoad(GameSaveData data)
    {
        if (data == null)
        {
            return CurrentChronoCharge;
        }

        if (data.SaveVersion <= 0)
        {
            return data.ChronoCharge > 0 ? data.ChronoCharge : CurrentChronoCharge;
        }

        return data.ChronoCharge;
    }

    private void RefreshItemLookup()
    {
        itemLookup.Clear();

        if (AllGameItems == null)
        {
            AllGameItems = new List<MergeableItemData>();
        }

        for (int i = 0; i < AllGameItems.Count; i++)
        {
            RegisterItemData(AllGameItems[i], "SaveManager.AllGameItems");
        }

        MarketManager[] marketManagers = FindObjectsOfType<MarketManager>();
        for (int i = 0; i < marketManagers.Length; i++)
        {
            MarketManager marketManager = marketManagers[i];
            if (marketManager == null || marketManager.marketItems == null)
            {
                continue;
            }

            for (int j = 0; j < marketManager.marketItems.Count; j++)
            {
                RegisterItemData(marketManager.marketItems[j], "MarketManager.marketItems");
            }
        }

        LevelManager[] levelManagers = FindObjectsOfType<LevelManager>();
        for (int i = 0; i < levelManagers.Length; i++)
        {
            LevelManager levelManager = levelManagers[i];
            if (levelManager == null)
            {
                continue;
            }

            RegisterItemData(levelManager.TargetItem, "LevelManager.TargetItem");
            RegisterItemData(levelManager.RewardItem, "LevelManager.RewardItem");
        }
    }

    private void RegisterItemData(MergeableItemData itemData, string source)
    {
        if (itemData == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(itemData.ItemID))
        {
            Debug.LogError($"KRITIK: {source} icindeki {itemData.name} objesinin ItemID'si bos.");
            return;
        }

        if (itemLookup.TryGetValue(itemData.ItemID, out MergeableItemData existingItem) && existingItem != itemData)
        {
            Debug.LogError($"KRITIK: Cakisan ID tespit edildi: {itemData.ItemID} ({existingItem.name} / {itemData.name})");
            return;
        }

        itemLookup[itemData.ItemID] = itemData;
    }

    public MergeableItemData GetItemDataByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (itemLookup.TryGetValue(id, out MergeableItemData itemData))
        {
            return itemData;
        }

        RefreshItemLookup();

        if (itemLookup.TryGetValue(id, out itemData))
        {
            return itemData;
        }

        for (int i = 0; i < AllGameItems.Count; i++)
        {
            MergeableItemData item = AllGameItems[i];
            if (item != null && item.ItemID == id)
            {
                return item;
            }
        }

        Debug.LogWarning($"Kayitli obje veritabaninda bulunamadi: {id}");
        return null;
    }

    private void SpawnObjectAt(MergeableItemData itemData, Vector2Int pos, GridManager gridManager)
    {
        if (!gridManager.IsValidPosition(pos))
        {
            Debug.LogWarning($"[SaveManager:SPAWN] Gecersiz pozisyon. Item={itemData?.ItemID} Pos={pos}");
            return;
        }

        if (gridManager.grid[pos.x, pos.y].TileView == null)
        {
            Debug.LogWarning($"[SaveManager:SPAWN] TileView yok. Item={itemData?.ItemID} Pos={pos}");
            return;
        }

        Vector3 worldPos = gridManager.grid[pos.x, pos.y].TileView.GetWorldPosition();
        if (itemData.Prefab == null)
        {
            Debug.LogError($"ItemData ({itemData.ItemID}) prefabi bos.");
            return;
        }

        GameObject newObj = Instantiate(itemData.Prefab, worldPos, Quaternion.identity);
        MergeableObject mergeable = newObj.GetComponent<MergeableObject>();

        if (mergeable == null)
        {
            Debug.LogError($"Prefab ({itemData.ItemID}) uzerinde MergeableObject yok.");
            Destroy(newObj);
            return;
        }

        mergeable.CurrentGridPosition = pos;
        mergeable.InitializeObject();
        Debug.Log($"[SaveManager:SPAWN] Basarili. Item={itemData.ItemID} Pos={pos}");
    }
}
