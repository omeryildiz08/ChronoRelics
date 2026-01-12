using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
  
    public List<TileSaveData> SavedTiles = new List<TileSaveData>();

    public List<string> InventoryItemIDs = new List<string>();
}

[System.Serializable]
public class TileSaveData
{
    public Vector2Int GridPos;
    public bool IsLocked;
    public string ItemID;
}