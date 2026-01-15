using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    public static AnomalyManager Instance { get; private set; }

    [Header("Anomaly Ayarları")]
    [Tooltip("Her birleştirmede anomali çıkma ihtimali (0-100 arası)")]
    public float anomalyChance = 80f; //test için yüksek normalde düşürülür

    [Header("Görsel Efektler")]
    [Tooltip("Kilitlenen karenin üzerinde çıkacak efekt (Particle System vb.)")]
    public GameObject riftEffectPrefab;
    public GameObject persistentRiftEffectPrefab;

    private GridManager gridManager;
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
    }
    void Start()
    {
        gridManager = GridManager.Instance;

        if (gridManager != null)
        {
            gridManager.OnMergeCompleted += OnMergeAction;
        }
    }

    private void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.OnMergeCompleted -= OnMergeAction;
        }
    }

    private void OnMergeAction(MergeableItemData data)
    {
        CheckAndTriggerAnomaly();
    }
    private void CheckAndTriggerAnomaly()
    {
        float roll = Random.Range(0f, 100f);
        Debug.Log($"Anomali Zarı: {roll} (Gereken: < {anomalyChance})");

        if (roll < anomalyChance)
        {
            CreateTimeRift();
        }
    }

    private void CreateTimeRift()
    {
        if (gridManager == null) return;

        List<Vector2Int> candidatePositions = new List<Vector2Int>();

        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                GridTileData tile = gridManager.grid[x, y];
                if (tile.TileView != null && !tile.isLocked)
                {
                    candidatePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (candidatePositions.Count > 0)
        {
            Vector2Int targetPos = candidatePositions[Random.Range(0, candidatePositions.Count)];
            
          
            LockTileAt(targetPos);
            
            Debug.Log($"ZAMAN YARIĞI OLUŞTU! Konum: {targetPos}");
        }
    }

    private void LockTileAt(Vector2Int pos)
    {
        
        gridManager.grid[pos.x, pos.y].isLocked = true;
        
        
        if (gridManager.grid[pos.x, pos.y].TileView != null)
        {
            gridManager.grid[pos.x, pos.y].TileView.UpdateVisuals(true);
            
            
            if (riftEffectPrefab != null)
            {
                Vector3 worldPos = gridManager.grid[pos.x, pos.y].TileView.GetWorldPosition();
                Instantiate(riftEffectPrefab, worldPos, Quaternion.identity);
                Instantiate(persistentRiftEffectPrefab, worldPos, Quaternion.identity);
            }
        }
    }
}
