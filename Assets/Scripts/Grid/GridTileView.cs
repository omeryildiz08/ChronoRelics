using Unity.VisualScripting;
using UnityEngine;

public class GridTileView : MonoBehaviour
{
    public Vector2Int GridPosition;
    public float objectYOffset = 0.8f;

    [Header("Kilit Ayarlari")]
    public bool StartLocked = false;

    public MeshRenderer MyMeshRenderer;
    public Material LockedMaterial;
    public Material NormalMaterial;

    private bool isRegistered;

    private void Awake()
    {
        TryRegisterTile();
    }

    private void Start()
    {
        TryRegisterTile();
    }

    private void TryRegisterTile()
    {
        if (isRegistered)
        {
            return;
        }

        GridManager gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            return;
        }

        if (!gridManager.IsValidPosition(GridPosition))
        {
            Debug.LogError($"GridPosition gecersiz: {GridPosition}");
            return;
        }

        gridManager.RegisterTile(this, GridPosition);
        isRegistered = true;

        if (StartLocked)
        {
            gridManager.LockTile(GridPosition);
        }
        else
        {
            UpdateVisuals(false);
        }
    }

    public Vector3 GetWorldPosition()
    {
        return new Vector3(
            transform.position.x,
            transform.position.y + objectYOffset,
            transform.position.z
        );
    }

    public void UpdateVisuals(bool isLocked)
    {
        if (MyMeshRenderer != null && LockedMaterial != null && NormalMaterial != null)
        {
            MyMeshRenderer.material = isLocked ? LockedMaterial : NormalMaterial;
        }
    }
}
