using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance { get; private set; }

    [Header("Type 1 - Random Rift")]
    [Tooltip("Her merge sonrası Tip 1 anomali çıkma ihtimali (0-100).")]
    [Range(0f, 100f)] public float anomalyChance = 25f;
    public bool enableType1 = true;

    [Header("Type 1 - Visuals")]
    public GameObject riftEffectPrefab;
    public GameObject persistentRiftEffectPrefab;

    [Header("Type 2 - Orb Rift")]
    public bool enableType2 = true;
    [Tooltip("Her merge sonrası her orb için aktivasyon ihtimali (0-100).")]
    [Range(0f, 100f)] public float type2ActivationChance = 20f;
    [Tooltip("Kapalıysa bir merge'de en fazla 1 orb aktivasyonu olur.")]
    public bool allowMultipleType2ActivationsPerMerge = true;
    [Tooltip("Seviye başlangıcında orb olacak tile'lar.")]
    public List<GridTileView> initialAnomalyOrbTiles = new List<GridTileView>();
    [Tooltip("Açıksa orb teleport sadece initialAnomalyOrbTiles listesinde dolaşır.")]
    public bool keepOrbsWithinConfiguredTiles = true;
    public GameObject anomalyOrbEffectPrefab;

    [Header("Type 2 - Effect Weights")]
    [Min(0f)] public float lockOpenTileWeight = 40f;
    [Min(0f)] public float swapLockedItemWeight = 25f;
    [Min(0f)] public float spawnForeignItemWeight = 20f;
    [Min(0f)] public float teleportOrbWeight = 15f;

    [Header("Type 2 - Foreign Era Spawn Pool")]
    public List<MergeableItemData> foreignEraItems = new List<MergeableItemData>();

    [Header("Type 2 - Debug")]
    public bool verboseType2Logs = true;
    public ForcedType2Effect forcedType2Effect = ForcedType2Effect.None;

    private GridManager gridManager;
    private readonly Dictionary<Vector2Int, List<GameObject>> type1TileEffects = new Dictionary<Vector2Int, List<GameObject>>();
    private readonly HashSet<Vector2Int> anomalyOrbPositions = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, GameObject> anomalyOrbVisuals = new Dictionary<Vector2Int, GameObject>();
    private readonly HashSet<Vector2Int> configuredOrbTilePositions = new HashSet<Vector2Int>();

    public enum ForcedType2Effect
    {
        None,
        LockOpenTile,
        SwapLockedItem,
        SpawnForeignItem,
        TeleportOrb
    }

    private enum Type2Effect
    {
        LockOpenTile,
        SwapLockedItem,
        SpawnForeignItem,
        TeleportOrb
    }

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
        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("AnomalyManager: GridManager bulunamadı.");
            enabled = false;
            return;
        }

        gridManager.OnMergeCompleted += OnMergeAction;
        gridManager.OnTileLockStateChanged += OnTileLockStateChanged;

        InitializeType2Orbs();
    }

    private void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.OnMergeCompleted -= OnMergeAction;
            gridManager.OnTileLockStateChanged -= OnTileLockStateChanged;
        }

        CleanupVisuals();
    }

    private void OnMergeAction(MergeableItemData _)
    {
        if (enableType1)
        {
            TryTriggerType1();
        }

        if (enableType2)
        {
            TryTriggerType2();
        }
    }

    private void OnTileLockStateChanged(Vector2Int pos, bool isLocked)
    {
        if (!isLocked)
        {
            ClearType1Effects(pos);
        }
    }

    private void TryTriggerType1()
    {
        float roll = Random.Range(0f, 100f);
        if (roll >= anomalyChance) return;

        List<Vector2Int> candidatePositions = new List<Vector2Int>();
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.TileView != null && !tile.isLocked && !tile.hasAnomaly)
                {
                    candidatePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (candidatePositions.Count == 0) return;

        Vector2Int targetPos = candidatePositions[Random.Range(0, candidatePositions.Count)];
        gridManager.LockTile(targetPos);
        SpawnType1Effects(targetPos);
        Debug.Log($"[Anomaly Type1] Tile kilitlendi: {targetPos}");
    }

    private void SpawnType1Effects(Vector2Int pos)
    {
        ClearType1Effects(pos);

        GridTileView tileView = gridManager.grid[pos.x, pos.y].TileView;
        if (tileView == null) return;

        Vector3 worldPos = tileView.GetWorldPosition();
        List<GameObject> spawned = new List<GameObject>();

        if (riftEffectPrefab != null)
        {
            GameObject vfx = Instantiate(riftEffectPrefab, worldPos, Quaternion.identity);
            spawned.Add(vfx);
        }

        if (persistentRiftEffectPrefab != null)
        {
            GameObject persistent = Instantiate(persistentRiftEffectPrefab, worldPos, Quaternion.identity);
            spawned.Add(persistent);
        }

        if (spawned.Count > 0)
        {
            type1TileEffects[pos] = spawned;
        }
    }

    private void ClearType1Effects(Vector2Int pos)
    {
        if (!type1TileEffects.TryGetValue(pos, out List<GameObject> effectList)) return;

        for (int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i] != null)
            {
                Destroy(effectList[i]);
            }
        }

        type1TileEffects.Remove(pos);
    }

    private void InitializeType2Orbs()
    {
        if (!enableType2) return;
        configuredOrbTilePositions.Clear();

        for (int i = 0; i < initialAnomalyOrbTiles.Count; i++)
        {
            GridTileView tile = initialAnomalyOrbTiles[i];
            if (tile == null) continue;

            configuredOrbTilePositions.Add(tile.GridPosition);
            RegisterOrbAt(tile.GridPosition);
        }
    }

    private void RegisterOrbAt(Vector2Int pos)
    {
        if (!gridManager.IsValidPosition(pos)) return;

        GridTileData tile = gridManager.grid[pos.x, pos.y];
        if (tile.TileView == null) return;
        if (anomalyOrbPositions.Contains(pos)) return;

        anomalyOrbPositions.Add(pos);
        tile.hasAnomaly = true;

        if (anomalyOrbEffectPrefab != null)
        {
            Vector3 worldPos = GetOrbWorldPosition(pos);
            GameObject orbVfx = Instantiate(anomalyOrbEffectPrefab, worldPos, Quaternion.identity);
            anomalyOrbVisuals[pos] = orbVfx;
        }
    }

    private void TryTriggerType2()
    {
        if (anomalyOrbPositions.Count == 0) return;

        List<Vector2Int> currentOrbs = new List<Vector2Int>(anomalyOrbPositions);
        if (!allowMultipleType2ActivationsPerMerge)
        {
            Vector2Int pickedOrb = currentOrbs[Random.Range(0, currentOrbs.Count)];
            float singleRoll = Random.Range(0f, 100f);
            if (singleRoll < type2ActivationChance)
            {
                ActivateType2Orb(pickedOrb);
            }
            else if (verboseType2Logs)
            {
                Debug.Log($"[Anomaly Type2] No activation this merge. Orb={pickedOrb}, Roll={singleRoll:0.00}, Threshold={type2ActivationChance:0.00}");
            }
            return;
        }

        for (int i = 0; i < currentOrbs.Count; i++)
        {
            Vector2Int orbPos = currentOrbs[i];
            if (!anomalyOrbPositions.Contains(orbPos)) continue;

            float roll = Random.Range(0f, 100f);
            if (roll < type2ActivationChance)
            {
                ActivateType2Orb(orbPos);
            }
        }
    }

    private void ActivateType2Orb(Vector2Int orbPos)
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            Type2Effect effect = GetRequestedType2Effect();
            if (ApplyType2Effect(effect, orbPos))
            {
                Debug.Log($"[Anomaly Type2] Orb {orbPos} effect: {effect}");
                return;
            }
        }

        Debug.Log($"[Anomaly Type2] Orb {orbPos} için uygulanabilir etki bulunamadı.");
    }

    private Type2Effect GetRequestedType2Effect()
    {
        switch (forcedType2Effect)
        {
            case ForcedType2Effect.LockOpenTile:
                return Type2Effect.LockOpenTile;
            case ForcedType2Effect.SwapLockedItem:
                return Type2Effect.SwapLockedItem;
            case ForcedType2Effect.SpawnForeignItem:
                return Type2Effect.SpawnForeignItem;
            case ForcedType2Effect.TeleportOrb:
                return Type2Effect.TeleportOrb;
            default:
                return RollType2Effect();
        }
    }

    private Type2Effect RollType2Effect()
    {
        float totalWeight = lockOpenTileWeight + swapLockedItemWeight + spawnForeignItemWeight + teleportOrbWeight;
        if (totalWeight <= 0f) return Type2Effect.LockOpenTile;

        float roll = Random.Range(0f, totalWeight);
        if (roll < lockOpenTileWeight) return Type2Effect.LockOpenTile;

        roll -= lockOpenTileWeight;
        if (roll < swapLockedItemWeight) return Type2Effect.SwapLockedItem;

        roll -= swapLockedItemWeight;
        if (roll < spawnForeignItemWeight) return Type2Effect.SpawnForeignItem;

        return Type2Effect.TeleportOrb;
    }

    private bool ApplyType2Effect(Type2Effect effect, Vector2Int orbPos)
    {
        switch (effect)
        {
            case Type2Effect.LockOpenTile:
                return ApplyLockOpenTileEffect(orbPos);
            case Type2Effect.SwapLockedItem:
                return ApplySwapLockedItemEffect(orbPos);
            case Type2Effect.SpawnForeignItem:
                return ApplySpawnForeignItemEffect(orbPos);
            case Type2Effect.TeleportOrb:
                return ApplyTeleportOrbEffect(orbPos);
            default:
                return false;
        }
    }

    private bool ApplyLockOpenTileEffect(Vector2Int center)
    {
        List<Vector2Int> area = GetAreaPositions(center, 1);
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int i = 0; i < area.Count; i++)
        {
            Vector2Int pos = area[i];
            GridTileData tile = gridManager.grid[pos.x, pos.y];
            if (tile.TileView != null && !tile.isLocked && !tile.hasAnomaly)
            {
                candidates.Add(pos);
            }
        }

        if (candidates.Count == 0) return false;

        Vector2Int target = candidates[Random.Range(0, candidates.Count)];
        gridManager.LockTile(target);
        SpawnType1Effects(target);
        if (verboseType2Logs)
        {
            Debug.Log($"[Anomaly Type2][Lock] Orb={center} LockedTile={target}");
        }
        return true;
    }

    private bool ApplySwapLockedItemEffect(Vector2Int center)
    {
        List<Vector2Int> area = GetAreaPositions(center, 1);
        List<MergeableObject> lockedObjects = new List<MergeableObject>();
        List<Vector2Int> openTargets = new List<Vector2Int>();

        for (int i = 0; i < area.Count; i++)
        {
            Vector2Int pos = area[i];
            GridTileData tile = gridManager.grid[pos.x, pos.y];
            if (tile.TileView == null) continue;

            if (tile.isLocked && tile.ObjectOnTile != null)
            {
                lockedObjects.Add(tile.ObjectOnTile);
            }
            else if (!tile.isLocked && tile.IsEmpty && !tile.hasAnomaly)
            {
                openTargets.Add(pos);
            }
        }

        if (lockedObjects.Count == 0 || openTargets.Count == 0) return false;

        MergeableObject sourceObj = lockedObjects[Random.Range(0, lockedObjects.Count)];
        Vector2Int destination = openTargets[Random.Range(0, openTargets.Count)];
        Vector2Int sourcePos = sourceObj.CurrentGridPosition;
        bool moved = gridManager.ForceMoveObject(sourceObj, destination);
        if (moved && verboseType2Logs)
        {
            Debug.Log($"[Anomaly Type2][SwapLockedItem] Orb={center} From={sourcePos} To={destination} Item={sourceObj.ItemData?.ItemID}");
        }
        return moved;
    }

    private bool ApplySpawnForeignItemEffect(Vector2Int center)
    {
        if (foreignEraItems == null || foreignEraItems.Count == 0) return false;

        List<Vector2Int> area = GetAreaPositions(center, 1);
        List<Vector2Int> spawnTargets = new List<Vector2Int>();

        for (int i = 0; i < area.Count; i++)
        {
            Vector2Int pos = area[i];
            GridTileData tile = gridManager.grid[pos.x, pos.y];
            if (tile.TileView != null && !tile.isLocked && tile.IsEmpty && !tile.hasAnomaly)
            {
                spawnTargets.Add(pos);
            }
        }

        if (spawnTargets.Count == 0) return false;

        MergeableItemData selected = GetRandomForeignItem();
        if (selected == null || selected.Prefab == null) return false;

        Vector2Int target = spawnTargets[Random.Range(0, spawnTargets.Count)];
        Vector3 spawnPos = gridManager.grid[target.x, target.y].TileView.GetWorldPosition();

        GameObject newObj = Instantiate(selected.Prefab, spawnPos, selected.Prefab.transform.rotation);
        MergeableObject mergeable = newObj.GetComponent<MergeableObject>();
        if (mergeable == null)
        {
            Destroy(newObj);
            return false;
        }

        mergeable.CurrentGridPosition = target;
        mergeable.InitializeObject();
        mergeable.IsInactiveAnomalyItem = true;
        if (verboseType2Logs)
        {
            Debug.Log($"[Anomaly Type2][SpawnForeign] Orb={center} SpawnTile={target} Item={selected.ItemID}");
        }
        return true;
    }

    private MergeableItemData GetRandomForeignItem()
    {
        List<MergeableItemData> validItems = new List<MergeableItemData>();
        for (int i = 0; i < foreignEraItems.Count; i++)
        {
            if (foreignEraItems[i] != null && foreignEraItems[i].Prefab != null)
            {
                validItems.Add(foreignEraItems[i]);
            }
        }

        if (validItems.Count == 0) return null;
        return validItems[Random.Range(0, validItems.Count)];
    }

    private bool ApplyTeleportOrbEffect(Vector2Int currentOrbPos)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        if (keepOrbsWithinConfiguredTiles && configuredOrbTilePositions.Count > 0)
        {
            foreach (Vector2Int pos in configuredOrbTilePositions)
            {
                if (!gridManager.IsValidPosition(pos)) continue;
                GridTileData tile = gridManager.grid[pos.x, pos.y];
                if (tile.TileView == null) continue;
                if (tile.hasAnomaly) continue;
                candidates.Add(pos);
            }
        }
        else
        {
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    GridTileData tile = gridManager.grid[x, y];
                    if (tile.TileView == null) continue;
                    if (tile.hasAnomaly) continue;
                    candidates.Add(pos);
                }
            }
        }

        if (candidates.Count == 0) return false;

        Vector2Int targetPos = candidates[Random.Range(0, candidates.Count)];
        bool moved = MoveOrb(currentOrbPos, targetPos);
        if (moved && verboseType2Logs)
        {
            Debug.Log($"[Anomaly Type2][Teleport] From={currentOrbPos} To={targetPos}");
        }
        return moved;
    }

    private bool MoveOrb(Vector2Int fromPos, Vector2Int toPos)
    {
        if (fromPos == toPos) return false;
        if (!anomalyOrbPositions.Contains(fromPos)) return false;
        if (!gridManager.IsValidPosition(toPos)) return false;
        if (gridManager.grid[toPos.x, toPos.y].TileView == null) return false;

        anomalyOrbPositions.Remove(fromPos);
        gridManager.grid[fromPos.x, fromPos.y].hasAnomaly = false;

        anomalyOrbPositions.Add(toPos);
        gridManager.grid[toPos.x, toPos.y].hasAnomaly = true;

        if (anomalyOrbVisuals.TryGetValue(fromPos, out GameObject visual))
        {
            anomalyOrbVisuals.Remove(fromPos);
            if (visual != null)
            {
                visual.transform.position = GetOrbWorldPosition(toPos);
                anomalyOrbVisuals[toPos] = visual;
            }
        }
        else if (anomalyOrbEffectPrefab != null)
        {
            GameObject orbVfx = Instantiate(anomalyOrbEffectPrefab, GetOrbWorldPosition(toPos), Quaternion.identity);
            anomalyOrbVisuals[toPos] = orbVfx;
        }

        return true;
    }

    private List<Vector2Int> GetAreaPositions(Vector2Int center, int radius)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (gridManager.IsValidPosition(pos))
                {
                    result.Add(pos);
                }
            }
        }

        return result;
    }

    private Vector3 GetOrbWorldPosition(Vector2Int pos)
    {
        GridTileView tileView = gridManager.grid[pos.x, pos.y].TileView;
        if (tileView == null) return Vector3.zero;
        return tileView.transform.position + Vector3.up * 0.2f;
    }

    private void CleanupVisuals()
    {
        foreach (var kv in type1TileEffects)
        {
            List<GameObject> effects = kv.Value;
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] != null)
                {
                    Destroy(effects[i]);
                }
            }
        }
        type1TileEffects.Clear();

        foreach (var kv in anomalyOrbVisuals)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value);
            }
        }
        anomalyOrbVisuals.Clear();
    }
}
