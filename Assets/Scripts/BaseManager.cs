using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return StartCoroutine(WaitForGridInitialization());

        SaveManager.Instance.LoadGame();
        SaveManager.Instance.RebuildBaseScene();
        CheckInventoryAndSpawn();
    }

    private IEnumerator WaitForGridInitialization()
    {
        while (GridManager.Instance == null)
        {
            yield return null;
        }

        GridManager gridManager = GridManager.Instance;
        GridTileView[] tileViews = FindObjectsOfType<GridTileView>();
        int expectedTileCount = tileViews.Length;

        while (gridManager.CountRegisteredTiles() < expectedTileCount)
        {
            yield return null;
        }
    }

    private void CheckInventoryAndSpawn()
    {
        if (SaveManager.Instance.CurrentInventory.Count == 0) return;

        GridManager gridManager = GridManager.Instance;
        List<string> itemsToRemove = new List<string>();

        foreach (string itemID in SaveManager.Instance.CurrentInventory)
        {
            MergeableItemData itemData = SaveManager.Instance.GetItemDataByID(itemID);
            if (itemData == null) continue;

            Vector2Int? emptyPos = gridManager.GetFirstEmptyPosition();
            if (emptyPos.HasValue)
            {
                Debug.Log($"[BaseManager:INVENTORY-SPAWN] Item={itemID} Pos={emptyPos.Value}");
                SpawnObjectAt(itemData, emptyPos.Value, gridManager);
                itemsToRemove.Add(itemID);
            }
            else
            {
                Debug.LogWarning("Base Grid dolu! Odul yerlestirilemedi.");
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
            gridManager.NotifyBaseStateChanged();
            Debug.Log($"{itemsToRemove.Count} adet odul Base'e yerlestirildi.");
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
            Debug.Log($"[BaseManager:INVENTORY-SPAWN] Basarili. Item={itemData.ItemID} Pos={pos}");
        }
    }
}
