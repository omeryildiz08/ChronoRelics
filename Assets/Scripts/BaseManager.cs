using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SaveManager.Instance.LoadGame();
        CheckInventoryAndSpawn();
    }

    private void CheckInventoryAndSpawn()
    {
        if (SaveManager.Instance.CurrentInventory.Count == 0) return;

        GridManager gridManager = GridManager.Instance;
        List<string> itemsToRemove = new List<string>();

        foreach (string itemID in SaveManager.Instance.CurrentInventory)
        {
            // ID'den Data'yı bul
            MergeableItemData itemData = SaveManager.Instance.GetItemDataByID(itemID);
            if (itemData == null) continue;

            
            Vector2Int? emptyPos = gridManager.GetFirstEmptyPosition();

            if (emptyPos.HasValue)
            {
                
                SpawnObjectAt(itemData, emptyPos.Value, gridManager);

               
                itemsToRemove.Add(itemID);
            }
            else
            {
                Debug.LogWarning("Base Grid dolu! Ödül yerleştirilemedi.");
               
                break;
            }
        }

        
        foreach (var item in itemsToRemove)
        {
            SaveManager.Instance.CurrentInventory.Remove(item);
        }

        if (itemsToRemove.Count > 0)
        {
            SaveManager.Instance.SaveGame();
            Debug.Log($"{itemsToRemove.Count} adet ödül Base'e yerleştirildi.");
        }
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
    }


}
