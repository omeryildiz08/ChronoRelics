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
    public AudioClip objectGrabSoundClip;
    public AudioSource audioSource;

    public GridTileData[,] grid;

    public event Action<MergeableItemData> OnMergeCompleted;
    public event Action<Vector2Int, bool> OnTileLockStateChanged;
    public event Action OnBaseStateChanged;
    [Header("Merge Animation")]
    [SerializeField] private MergeAnimationController mergeAnimationController;

    private bool isProcessingMerge;
    public bool IsProcessingMerge => isProcessingMerge;
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

        if (mergeAnimationController == null)
        {
            mergeAnimationController = GetComponent<MergeAnimationController>();
        }

        if (mergeAnimationController == null)
        {
            mergeAnimationController = FindObjectOfType<MergeAnimationController>();
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

    public void PlayObjectGrabSound()
    {
        if (audioSource != null && objectGrabSoundClip != null)
        {
            audioSource.PlayOneShot(objectGrabSoundClip);
        }
    }


    public void TryMergeOrPlace(MergeableObject movingObject, Vector2Int fromPos, Vector2Int toPos)
    {

        if (movingObject == null) return;

        if (isProcessingMerge)
        {
            SnapObjectToPosition(movingObject, fromPos);
            return;
        }

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
    public GridTileView GetTileView(Vector2Int position)
    {
        if (!IsValidPosition(position))
        {
            return null;
        }

        return grid[position.x, position.y].TileView;
    }

    public TileHighlightState GetDropHighlightState(
    MergeableObject movingObject,
    Vector2Int fromPos,
    Vector2Int targetPos)
    {
        if (movingObject == null)
        {
            return TileHighlightState.Invalid;
        }

        if (!IsValidPosition(targetPos))
        {
            return TileHighlightState.Invalid;
        }

        GridTileData targetTile = grid[targetPos.x, targetPos.y];

        if (targetTile.TileView == null)
        {
            return TileHighlightState.Invalid;
        }

        if (targetTile.isLocked)
        {
            return TileHighlightState.Invalid;
        }

        if (targetPos == fromPos)
        {
            return TileHighlightState.Origin;
        }

        MergeableObject targetObject = targetTile.ObjectOnTile;

        if (targetObject == null)
        {
            return TileHighlightState.Valid;
        }

        if (targetObject == movingObject)
        {
            return TileHighlightState.Origin;
        }

        if (targetObject.ItemData == movingObject.ItemData)
        {
            return TileHighlightState.MergeCandidate;
        }

        return TileHighlightState.Invalid;
    }

    private void PerformMerge(List<MergeableObject> mergeGroup, Vector2Int mergeCenterPos)
    {
        if (isProcessingMerge) return;
        isProcessingMerge = true;
        StartCoroutine(PerformMergeRoutine(mergeGroup, mergeCenterPos));
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
        NotifyBaseStateChanged();
    }

    public void NotifyBaseStateChanged()
    {
        if (SceneManager.GetActiveScene().name != "BaseScene") return;

        OnBaseStateChanged?.Invoke();
    }
    private System.Collections.IEnumerator PerformMergeRoutine(
        List<MergeableObject> mergeGroup,
        Vector2Int mergeCenterPos)
    {
        if (mergeGroup == null || mergeGroup.Count == 0)
        {
            EndMergeProcessing();
            yield break;
        }

        if (!IsValidPosition(mergeCenterPos) || grid[mergeCenterPos.x, mergeCenterPos.y].TileView == null)
        {
            EndMergeProcessing();
            yield break;
        }

        MergeableObject firstObject = mergeGroup[0];

        if (firstObject == null || firstObject.ItemData == null)
        {
            EndMergeProcessing();
            yield break;
        }

        MergeableItemData nextLevelData = firstObject.ItemData.NextLevelItem;

        if (nextLevelData == null || nextLevelData.Prefab == null)
        {
            EndMergeProcessing();
            yield break;
        }

        GridTileView centerTile = grid[mergeCenterPos.x, mergeCenterPos.y].TileView;
        Vector3 mergeCenterWorldPos = centerTile.GetWorldPosition();

        List<MergeableObject> validMergeObjects = new List<MergeableObject>();

        for (int i = 0; i < mergeGroup.Count; i++)
        {
            MergeableObject obj = mergeGroup[i];

            if (obj == null)
            {
                continue;
            }

            validMergeObjects.Add(obj);

            DisableObjectInteraction(obj);

            UnlockTile(obj.CurrentGridPosition);
            ClearCell(obj.CurrentGridPosition);
        }

        if (validMergeObjects.Count == 0)
        {
            EndMergeProcessing();
            yield break;
        }

        bool gatherAnimationCompleted = false;

        if (mergeAnimationController != null)
        {
            mergeAnimationController.PlayGatherAnimation(
                validMergeObjects,
                mergeCenterWorldPos,
                () => gatherAnimationCompleted = true
            );

            yield return new WaitUntil(() => gatherAnimationCompleted);
        }
        else
        {
            Debug.LogWarning("MergeAnimationController atanmadı. Merge animasyonsuz çalışacak.");
            PlayMergeFeedback(mergeCenterWorldPos);
        }

        for (int i = 0; i < validMergeObjects.Count; i++)
        {
            if (validMergeObjects[i] != null)
            {
                Destroy(validMergeObjects[i].gameObject);
            }
        }

        GameObject newObj = Instantiate(
            nextLevelData.Prefab,
            mergeCenterWorldPos,
            nextLevelData.Prefab.transform.rotation
        );

        MergeableObject newMergeable = newObj.GetComponent<MergeableObject>();

        if (newMergeable == null)
        {
            Debug.LogError("Yeni oluşturulan obje üzerinde MergeableObject bileşeni bulunamadı!");
            Destroy(newObj);
            EndMergeProcessing();
            yield break;
        }

        newMergeable.CurrentGridPosition = mergeCenterPos;
        RegisterObject(newMergeable, mergeCenterPos);

        bool popAnimationCompleted = false;

        if (mergeAnimationController != null)
        {
            mergeAnimationController.PlayNewObjectPopAnimation(
                newObj.transform,
                () => popAnimationCompleted = true
            );

            yield return new WaitUntil(() => popAnimationCompleted);
        }

        OnMergeCompleted?.Invoke(newMergeable.ItemData);
        SaveBaseStateIfNeeded();

        EndMergeProcessing();
    }

    private void DisableObjectInteraction(MergeableObject obj)
    {
        if (obj == null)
        {
            return;
        }

        Collider objCollider = obj.GetComponent<Collider>();

        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
    }

    private void EndMergeProcessing()
    {
        isProcessingMerge = false;
    }

    private void PlayMergeFeedback(Vector3 position)
    {
        if (mergeVFXPrefab != null)
        {
            GameObject vfx = Instantiate(mergeVFXPrefab, position, Quaternion.identity);
            Destroy(vfx, 2.0f);
        }

        if (audioSource != null && mergeSoundClip != null)
        {
            audioSource.PlayOneShot(mergeSoundClip);
        }
    }
}
