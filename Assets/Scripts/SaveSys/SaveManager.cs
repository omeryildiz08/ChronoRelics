using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; } //singleton 

    [Header("Veri Tabanı (Referanslar)")]
    public List<MergeableItemData> AllGameItems;
    private string saveFilePath;

    public List<string> CurrentInventory = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        ValidateDatabase();

    }
    private void ValidateDatabase()
    {
        HashSet<string> ids = new HashSet<string>();
        foreach (var item in AllGameItems)
        {
            if(item == null) continue;
           
            if(string.IsNullOrEmpty(item.ItemID)) 
                Debug.LogError($"KRİTİK: {item.name} objesinin ItemID'si boş!");
            else if(ids.Contains(item.ItemID))
                Debug.LogError($"KRİTİK: Çakışan ID tespit edildi: {item.ItemID}");
            
            ids.Add(item.ItemID);
        }
    }
    public void AddRewardToInventory(string itemID)
    {
        if(!string.IsNullOrEmpty(itemID))
        {
            CurrentInventory.Add(itemID);
            Debug.Log($"Envantere eklendi: {itemID}");
            SaveGame();
        }
    }
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "BaseScene") 
        {
            SaveInventoryOnly();
            return;
        }
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("GridManager bulunamadı, kaydetme işlemi iptal edildi.");
            return;
        }

        GameSaveData data = new GameSaveData();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.IsEmpty && !tile.isLocked) continue;
                TileSaveData tileData = new TileSaveData();
                tileData.GridPos = new Vector2Int(x, y);
                tileData.IsLocked = tile.isLocked;
                if (tile.ObjectOnTile != null)
                {
                    // Objenin adını (ID) kaydet
                    tileData.ItemID = tile.ObjectOnTile.ItemData.ItemID;
                }

                else
                {
                    tileData.ItemID = ""; 
                }

                data.SavedTiles.Add(tileData);
            }

        }
        data.InventoryItemIDs = new List<string>(CurrentInventory);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
       Debug.Log($"Base Durumu Kaydedildi: {saveFilePath}");
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Kayit dosyasi bulunamadi: " + saveFilePath);
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if(gridManager == null) return; 

        ClearCurrentScene(gridManager);

        string json = File.ReadAllText(saveFilePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        
        CurrentInventory = new List<string>(data.InventoryItemIDs);
        //gridi yeniden inşa
        foreach (TileSaveData tileData in data.SavedTiles)
        {
            Vector2Int pos = tileData.GridPos;
            
            if (tileData.IsLocked)
            {
                gridManager.LockTile(pos);
            }
            else
            {
                
                gridManager.UnlockTile(pos);
            }

            if (!string.IsNullOrEmpty(tileData.ItemID))
            {
              
                MergeableItemData itemData = GetItemDataByID(tileData.ItemID);
                
                if (itemData != null)
                {
                    // Yarat ve Grid'e Kaydet
                    SpawnObjectAt(itemData, pos, gridManager);
                }
                else
                {
                    Debug.LogWarning($"Kayıtlı obje veritabanında bulunamadı: {tileData.ItemID}");
                }
            }
        }
        Debug.Log("Oyun Yuklendi: " + saveFilePath);
        Debug.Log($"Oyun Yüklendi! Envanterde {CurrentInventory.Count} eşya var.");

    }
    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Save dosyası silindi! Temiz başlangıç.");
        }
    }

    private void ClearCurrentScene(GridManager gridManager)
    {
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                if (gridManager.grid[x, y].ObjectOnTile != null)
                {
                    Destroy(gridManager.grid[x, y].ObjectOnTile.gameObject);
                    gridManager.grid[x, y].ObjectOnTile = null;
                }

                
                
             
                gridManager.UnlockTile(new Vector2Int(x,y));
            }
        }
    }
    private void SaveInventoryOnly()
    {
        GameSaveData data ; 
        if (File.Exists(saveFilePath))
        {
            string existingJson = File.ReadAllText(saveFilePath);
            data = JsonUtility.FromJson<GameSaveData>(existingJson);
        }
        else
        {
          
            data = new GameSaveData();
        }
        data.InventoryItemIDs = new List<string>(CurrentInventory);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        
        Debug.Log("Sadece Envanter Güncellendi (Level Modu).");
    }

    public MergeableItemData GetItemDataByID(string id)
    {
        foreach (var item in AllGameItems)
        {
            if (item!=null && item.ItemID == id) return item;
        }
        Debug.LogWarning($"Kayıtlı obje veritabanında bulunamadı: {id}");
        return null;
    }

    private void SpawnObjectAt(MergeableItemData itemData, Vector2Int pos, GridManager gm)
    {
        if (gm.grid[pos.x, pos.y].TileView == null) return;
        Vector3 worldPos = gm.grid[pos.x, pos.y].TileView.GetWorldPosition();
        
        if(itemData.Prefab == null)
        {
             Debug.LogError($"ItemData ({itemData.ItemID}) prefabı boş!");
             return;
        }
        
       
        GameObject newObj = Instantiate(itemData.Prefab, worldPos, Quaternion.identity);
        MergeableObject mergeable = newObj.GetComponent<MergeableObject>();
        
        if(mergeable != null)
        {
            mergeable.CurrentGridPosition = pos;
            
           
            mergeable.InitializeObject(); 
        }
        else
        {
             Debug.LogError($"Prefab ({itemData.ItemID}) üzerinde MergeableObject scripti yok!");
             Destroy(newObj);
        }
        
        
    }
}


