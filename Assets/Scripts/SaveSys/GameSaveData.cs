using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public int SaveVersion = 0;

    // Legacy field kept for backward compatibility with existing save files.
    public List<TileSaveData> SavedTiles = new List<TileSaveData>();

    public List<SavedObjectData> SavedObjects = new List<SavedObjectData>();
    public List<LockedTileSaveData> LockedTiles = new List<LockedTileSaveData>();
    public List<UnlockedSceneLockedTileSaveData> UnlockedSceneLockedTiles = new List<UnlockedSceneLockedTileSaveData>();
    public List<string> InventoryItemIDs = new List<string>();
    public int TimeCredits = 0;
    public int ChronoCharge = 0;
}

[System.Serializable]
public class SavedObjectData
{
    public Vector2Int GridPos;
    public string ItemID;
}

[System.Serializable]
public class LockedTileSaveData
{
    public Vector2Int GridPos;
}

[System.Serializable]
public class UnlockedSceneLockedTileSaveData
{
    public Vector2Int GridPos;
}

[System.Serializable]
public class TileSaveData
{
    public Vector2Int GridPos;
    public bool IsLocked;
    public string ItemID;
}
