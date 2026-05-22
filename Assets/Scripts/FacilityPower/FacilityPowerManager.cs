using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FacilityPowerManager : MonoBehaviour
{
    public static FacilityPowerManager Instance { get; private set; }

    [Serializable]
    public class FacilityExpansionGroup
    {
        public string GroupId;
        [Tooltip("Bu gruptaki her tile'i acmak icin gereken kullanilabilir Facility Power.")]
        [Min(0)] public int RequiredPower;
        public List<GridTileView> TilesToUnlock = new List<GridTileView>();
    }

    [Header("Expansion")]
    public List<FacilityExpansionGroup> expansionGroups = new List<FacilityExpansionGroup>();

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;
    public bool playErrorSoundOnFailedUnlock = true;
    [Min(0f)] public float feedbackVisibleSeconds = 1.5f;
    public string unlockSuccessMessage = "Yeni tile acildi.";

    [Header("Unlock VFX")]
    public GameObject unlockVFXPrefab;
    [Min(0.1f)] public float unlockVFXLifetime = 2f;

    public int CurrentFacilityPower { get; private set; }
    public int UsedFacilityPower { get; private set; }
    public int AvailableFacilityPower { get; private set; }
    public event Action<int, int, int, int, bool, int> OnFacilityPowerChanged;

    private GridManager gridManager;
    private bool recalculateQueued;
    private Coroutine feedbackRoutine;
    private readonly HashSet<GridTileView> unlocksInProgress = new HashSet<GridTileView>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        if (SceneManager.GetActiveScene().name != "BaseScene")
        {
            enabled = false;
            yield break;
        }

        while (GridManager.Instance == null)
        {
            yield return null;
        }

        gridManager = GridManager.Instance;
        gridManager.OnBaseStateChanged += QueueRecalculate;
        gridManager.OnTileLockStateChanged += OnTileLockStateChanged;

        yield return null;
        RecalculateFacilityPower();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (gridManager != null)
        {
            gridManager.OnBaseStateChanged -= QueueRecalculate;
            gridManager.OnTileLockStateChanged -= OnTileLockStateChanged;
        }
    }

    public bool TryUnlockTile(GridTileView tileView)
    {
        if (tileView == null || gridManager == null)
        {
            return false;
        }

        Vector2Int pos = tileView.GridPosition;
        if (!gridManager.IsValidPosition(pos))
        {
            return false;
        }

        GridTileData tileData = gridManager.grid[pos.x, pos.y];
        if (!tileData.isLocked)
        {
            return false;
        }

        FacilityExpansionGroup group = FindExpansionGroupForTile(tileView);
        if (group == null)
        {
            return false;
        }

        int unlockCost = GetUnlockCost(group);
        if (AvailableFacilityPower < unlockCost)
        {
            int missingPower = unlockCost - AvailableFacilityPower;
            SetFeedback($"Tile acmak icin {unlockCost} FP gerekli. Kullanilabilir: {AvailableFacilityPower} ({missingPower} FP eksik)");
            PlayFailedUnlockFeedback();
            return true;
        }

        if (unlocksInProgress.Contains(tileView))
        {
            return true;
        }

        StartCoroutine(UnlockTileRoutine(tileView, pos));
        return true;
    }

    public void RecalculateFacilityPower()
    {
        if (gridManager == null)
        {
            gridManager = GridManager.Instance;
        }

        if (gridManager == null)
        {
            return;
        }

        int totalPower = 0;
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                MergeableObject obj = tile.ObjectOnTile;
                if (tile.TileView == null || obj == null || obj.ItemData == null)
                {
                    continue;
                }

                totalPower += Mathf.Max(0, obj.ItemData.FacilityPoint);
            }
        }

        CurrentFacilityPower = totalPower;
        UsedFacilityPower = CalculateUsedFacilityPower();
        AvailableFacilityPower = Mathf.Max(0, CurrentFacilityPower - UsedFacilityPower);
        RefreshExpansionVisuals();
        NotifyPowerChanged();
    }

    private IEnumerator UnlockTileRoutine(GridTileView tileView, Vector2Int pos)
    {
        unlocksInProgress.Add(tileView);
        ClearFeedback();

        bool animationComplete = false;
        tileView.PlayUnlockAnimation(() => animationComplete = true);

        while (!animationComplete)
        {
            yield return null;
        }

        gridManager.UnlockTile(pos);
        tileView.SetUnlockableVisual(false);
        SpawnUnlockVFX(tileView);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        gridManager.NotifyBaseStateChanged();
        unlocksInProgress.Remove(tileView);
        RecalculateFacilityPower();
        SetFeedback(unlockSuccessMessage);
    }

    private void QueueRecalculate()
    {
        if (recalculateQueued || !isActiveAndEnabled)
        {
            return;
        }

        StartCoroutine(RecalculateNextFrame());
    }

    private IEnumerator RecalculateNextFrame()
    {
        recalculateQueued = true;
        yield return null;
        recalculateQueued = false;
        RecalculateFacilityPower();
    }

    private void OnTileLockStateChanged(Vector2Int pos, bool isLocked)
    {
        QueueRecalculate();
    }

    private void RefreshExpansionVisuals()
    {
        for (int i = 0; i < expansionGroups.Count; i++)
        {
            FacilityExpansionGroup group = expansionGroups[i];
            if (group == null || group.TilesToUnlock == null)
            {
                continue;
            }

            bool isUnlockable = AvailableFacilityPower >= GetUnlockCost(group);
            for (int j = 0; j < group.TilesToUnlock.Count; j++)
            {
                GridTileView tileView = group.TilesToUnlock[j];
                if (tileView == null || gridManager == null)
                {
                    continue;
                }

                Vector2Int pos = tileView.GridPosition;
                if (!gridManager.IsValidPosition(pos))
                {
                    continue;
                }

                bool isLocked = gridManager.grid[pos.x, pos.y].isLocked;
                tileView.SetUnlockableVisual(isLocked && isUnlockable);
            }
        }
    }

    private void NotifyPowerChanged()
    {
        int unlockableTileCount = CountUnlockableTiles();
        int nextRequiredPower = GetNextUnlockCost(out bool hasNextThreshold);
        OnFacilityPowerChanged?.Invoke(
            CurrentFacilityPower,
            UsedFacilityPower,
            AvailableFacilityPower,
            nextRequiredPower,
            hasNextThreshold,
            unlockableTileCount);
    }

    private int CountUnlockableTiles()
    {
        int count = 0;
        for (int i = 0; i < expansionGroups.Count; i++)
        {
            FacilityExpansionGroup group = expansionGroups[i];
            if (group == null || AvailableFacilityPower < GetUnlockCost(group) || group.TilesToUnlock == null)
            {
                continue;
            }

            for (int j = 0; j < group.TilesToUnlock.Count; j++)
            {
                GridTileView tileView = group.TilesToUnlock[j];
                if (IsTileLocked(tileView))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private int GetNextUnlockCost(out bool hasNextThreshold)
    {
        hasNextThreshold = false;
        int nextCost = int.MaxValue;

        for (int i = 0; i < expansionGroups.Count; i++)
        {
            FacilityExpansionGroup group = expansionGroups[i];
            if (group == null || group.TilesToUnlock == null || !GroupHasLockedTile(group))
            {
                continue;
            }

            int unlockCost = GetUnlockCost(group);
            if (AvailableFacilityPower >= unlockCost)
            {
                continue;
            }

            if (unlockCost < nextCost)
            {
                nextCost = unlockCost;
                hasNextThreshold = true;
            }
        }

        return hasNextThreshold ? nextCost : 0;
    }

    private int CalculateUsedFacilityPower()
    {
        int usedPower = 0;
        HashSet<GridTileView> countedTiles = new HashSet<GridTileView>();

        for (int i = 0; i < expansionGroups.Count; i++)
        {
            FacilityExpansionGroup group = expansionGroups[i];
            if (group == null || group.TilesToUnlock == null)
            {
                continue;
            }

            int unlockCost = GetUnlockCost(group);
            for (int j = 0; j < group.TilesToUnlock.Count; j++)
            {
                GridTileView tileView = group.TilesToUnlock[j];
                if (tileView == null || countedTiles.Contains(tileView))
                {
                    continue;
                }

                if (IsExpansionTileUnlocked(tileView))
                {
                    usedPower += unlockCost;
                    countedTiles.Add(tileView);
                }
            }
        }

        return usedPower;
    }

    private bool IsExpansionTileUnlocked(GridTileView tileView)
    {
        if (tileView == null || gridManager == null || !tileView.StartLocked)
        {
            return false;
        }

        Vector2Int pos = tileView.GridPosition;
        return gridManager.IsValidPosition(pos) && !gridManager.grid[pos.x, pos.y].isLocked;
    }

    private int GetUnlockCost(FacilityExpansionGroup group)
    {
        return group != null ? Mathf.Max(0, group.RequiredPower) : 0;
    }

    private bool GroupHasLockedTile(FacilityExpansionGroup group)
    {
        for (int i = 0; i < group.TilesToUnlock.Count; i++)
        {
            if (IsTileLocked(group.TilesToUnlock[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTileLocked(GridTileView tileView)
    {
        if (tileView == null || gridManager == null)
        {
            return false;
        }

        Vector2Int pos = tileView.GridPosition;
        return gridManager.IsValidPosition(pos) && gridManager.grid[pos.x, pos.y].isLocked;
    }

    private FacilityExpansionGroup FindExpansionGroupForTile(GridTileView tileView)
    {
        for (int i = 0; i < expansionGroups.Count; i++)
        {
            FacilityExpansionGroup group = expansionGroups[i];
            if (group == null || group.TilesToUnlock == null)
            {
                continue;
            }

            if (group.TilesToUnlock.Contains(tileView))
            {
                return group;
            }
        }

        return null;
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (feedbackText != null && feedbackVisibleSeconds > 0f && !string.IsNullOrEmpty(message))
        {
            feedbackRoutine = StartCoroutine(ClearFeedbackAfterDelay());
        }
    }

    private void ClearFeedback()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    private IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackVisibleSeconds);
        ClearFeedback();
    }

    private void SpawnUnlockVFX(GridTileView tileView)
    {
        if (unlockVFXPrefab == null || tileView == null)
        {
            return;
        }

        GameObject vfx = Instantiate(unlockVFXPrefab, tileView.GetWorldPosition(), Quaternion.identity);
        Destroy(vfx, unlockVFXLifetime);
    }

    private void PlayFailedUnlockFeedback()
    {
        if (playErrorSoundOnFailedUnlock && gridManager != null)
        {
            gridManager.PlayErrorSound();
        }
    }
}
