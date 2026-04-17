using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{

    // Singleton 
    public static GridManager Instance { get; private set; }


    public int gridWidth = 50;
    public int gridHeight = 50;

    [Header("Görsel ve Ses Efektleri")]
    public GameObject mergeVFXPrefab;
    public AudioClip mergeSoundClip;
    public AudioClip errorSoundClip;
    public AudioSource audioSource;

    public GridTileData[,] grid;

    public event Action<MergeableItemData> OnMergeCompleted;
    public event Action<Vector2Int, bool> OnTileLockStateChanged;
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }


        grid = new GridTileData[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {

                grid[x, y] = new GridTileData();
            }
        }
    }


    public void RegisterTile(GridTileView tileView, Vector2Int position)
    {
        if (!IsValidPosition(position)) return;


        grid[position.x, position.y].TileView = tileView;
    }

    public int CountRegisteredTiles()
    {
        int count = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].TileView != null)
                {
                    count++;
                }
            }
        }

        return count;
    }


    public void RegisterObject(MergeableObject obj, Vector2Int position)
    {
        if (!IsValidPosition(position)) return;


        grid[position.x, position.y].ObjectOnTile = obj;
    }
    public void PlayErrorSound()
    {
        if (audioSource != null && errorSoundClip != null)
        {
            
            // audioSource.pitch = Random.Range(0.9f, 1.1f); 
            
            audioSource.PlayOneShot(errorSoundClip);
        }
    }


    public void TryMergeOrPlace(MergeableObject movingObject, Vector2Int fromPos, Vector2Int toPos)
    {
        if (movingObject == null) return;

        if (!IsValidPosition(toPos) || grid[toPos.x, toPos.y].TileView == null)
        {
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }
        if (IsTileLocked(toPos))
        {
            PlayErrorSound();
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }

        MergeableObject targetObj = grid[toPos.x, toPos.y].ObjectOnTile;


        if (targetObj != null && targetObj != movingObject)
        {

            if (targetObj.ItemData == movingObject.ItemData)
            {
                List<MergeableObject> mergeGroup = FindMergeGroup(toPos, movingObject.ItemData);

                if (!mergeGroup.Contains(movingObject)) mergeGroup.Add(movingObject);

                if (mergeGroup.Count >= 3)
                {
                    PerformMerge(mergeGroup, toPos);
                    return;
                }
            }
            // Tip farklıysa veya sayı yetmediyse geri dön
            SnapObjectToPosition(movingObject, fromPos);
        }
        // SENARYO 2: BOŞ BİR KAREYE BIRAKTIK 
        else
        {

            ClearCell(fromPos);
            RegisterObject(movingObject, toPos);
            movingObject.CurrentGridPosition = toPos;


            List<MergeableObject> mergeGroup = FindMergeGroup(toPos, movingObject.ItemData);

            if (mergeGroup.Count >= 3)
            {
                PerformMerge(mergeGroup, toPos);
            }
            else
            {

                SnapObjectToPosition(movingObject, toPos);
                SaveBaseStateIfNeeded();
            }
        }
    }
    public Vector2Int? GetFirstEmptyPosition()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {

                if (IsValidPosition(new Vector2Int(x, y)) &&
                    grid[x, y].TileView != null &&
                    grid[x, y].IsEmpty &&
                    !grid[x, y].isLocked)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }

    private void PerformMerge(List<MergeableObject> mergeGroup, Vector2Int mergeCenterPos)
    {
        if (mergeGroup == null || mergeGroup.Count == 0) return;
        MergeableItemData nextLevelData = mergeGroup[0].ItemData.NextLevelItem;
        if (nextLevelData == null) return;

        foreach (var obj in mergeGroup)
        {
            if (obj == null) continue;
            UnlockTile(obj.CurrentGridPosition);
            ClearCell(obj.CurrentGridPosition);
            Destroy(obj.gameObject);
        }


        if (grid[mergeCenterPos.x, mergeCenterPos.y].TileView != null)
        {
            Vector3 spawnPos = grid[mergeCenterPos.x, mergeCenterPos.y].TileView.GetWorldPosition();
            if (mergeVFXPrefab != null)
            {
                GameObject vfx = Instantiate(mergeVFXPrefab, spawnPos, Quaternion.identity);
                Destroy(vfx, 2.0f);
            }
            if (audioSource != null && mergeSoundClip != null)
            {
                
                audioSource.PlayOneShot(mergeSoundClip);
            }
            GameObject newObj = Instantiate(nextLevelData.Prefab, spawnPos,nextLevelData.Prefab.transform.rotation);

            MergeableObject newMergeable = newObj.GetComponent<MergeableObject>();
            if (newMergeable != null)
            {
                newMergeable.CurrentGridPosition = mergeCenterPos;
                RegisterObject(newMergeable, mergeCenterPos);

                OnMergeCompleted?.Invoke(newMergeable.ItemData);
                SaveBaseStateIfNeeded();
                // İleride 
                // buraya "CheckForCombo(newMergeable)" ekleyebiliriz.
            }
            else
            {
                Debug.LogError("Yeni oluşturulan obje üzerinde MergeableObject bileşeni bulunamadı!");
            }

        }
    }

    private void MoveObject(MergeableObject obj, Vector2Int fromPos, Vector2Int toPos)
    {
        ClearCell(fromPos);
        RegisterObject(obj, toPos);


        obj.CurrentGridPosition = toPos;


        SnapObjectToPosition(obj, toPos);
    }

    public void SnapObjectToPosition(MergeableObject obj, Vector2Int pos)
    {
        if (!IsValidPosition(pos) || grid[pos.x, pos.y].TileView == null) return;


        obj.transform.position = grid[pos.x, pos.y].TileView.GetWorldPosition();
    }

    private void ClearCell(Vector2Int pos)
    {
        if (IsValidPosition(pos))
        {
            grid[pos.x, pos.y].ObjectOnTile = null;
        }
    }

    private List<MergeableObject> FindMergeGroup(Vector2Int startPos, MergeableItemData targetData)
    {
        List<MergeableObject> group = new List<MergeableObject>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();

        toCheck.Enqueue(startPos);
        visited.Add(startPos);

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            MergeableObject obj = grid[current.x, current.y].ObjectOnTile;


            if (obj != null && obj.ItemData == targetData)
            {
                group.Add(obj);
            }

            else if (current != startPos)
            {
                continue;
            }

            // 4 Yöne Bak (Zinciri takip et)
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in directions)
            {
                Vector2Int neighborPos = current + dir;


                if (IsValidPosition(neighborPos) && !visited.Contains(neighborPos))
                {

                    MergeableObject neighborObj = grid[neighborPos.x, neighborPos.y].ObjectOnTile;
                    if (neighborObj != null && neighborObj.ItemData == targetData)
                    {
                        visited.Add(neighborPos);
                        toCheck.Enqueue(neighborPos);
                    }
                }
            }
        }
        return group;
    }
    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth &&
               pos.y >= 0 && pos.y < gridHeight;
    }
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x),
            Mathf.RoundToInt(worldPosition.z)
        );
    }

    public bool IsTileLocked(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return false;
        return grid[pos.x, pos.y].isLocked;
    }

    public void UnlockTile(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return;
        if (grid[pos.x, pos.y].TileView == null) return;

        if (!grid[pos.x, pos.y].isLocked) return;

        grid[pos.x, pos.y].isLocked = false;

        grid[pos.x, pos.y].TileView.UpdateVisuals(false);
        OnTileLockStateChanged?.Invoke(pos, false);
    }

    public void LockTile(Vector2Int pos)
    {
        if (!IsValidPosition(pos)) return;
        if (grid[pos.x, pos.y].TileView == null) return;

        if (grid[pos.x, pos.y].isLocked) return;

        grid[pos.x, pos.y].isLocked = true;
        grid[pos.x, pos.y].TileView.UpdateVisuals(true);
        OnTileLockStateChanged?.Invoke(pos, true);
    }

    public bool ForceMoveObject(MergeableObject obj, Vector2Int toPos)
    {
        if (obj == null) return false;
        if (!IsValidPosition(toPos)) return false;
        if (grid[toPos.x, toPos.y].TileView == null) return false;
        if (grid[toPos.x, toPos.y].ObjectOnTile != null) return false;
        if (grid[toPos.x, toPos.y].isLocked) return false;

        Vector2Int fromPos = obj.CurrentGridPosition;
        if (IsValidPosition(fromPos) && grid[fromPos.x, fromPos.y].ObjectOnTile == obj)
        {
            ClearCell(fromPos);
        }

        RegisterObject(obj, toPos);
        obj.CurrentGridPosition = toPos;
        SnapObjectToPosition(obj, toPos);
        SaveBaseStateIfNeeded();
        return true;
    }

    public bool RemoveObject(MergeableObject obj)
    {
        if (obj == null) return false;

        Vector2Int pos = obj.CurrentGridPosition;
        if (!IsValidPosition(pos)) return false;
        if (grid[pos.x, pos.y].ObjectOnTile != obj) return false;

        ClearCell(pos);
        Destroy(obj.gameObject);
        SaveBaseStateIfNeeded();
        return true;
    }

    private void SaveBaseStateIfNeeded()
    {
        if (SaveManager.Instance == null) return;
        if (SceneManager.GetActiveScene().name != "BaseScene") return;

        SaveManager.Instance.SaveGame();
    }
}
