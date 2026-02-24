using Unity.VisualScripting;
using UnityEngine;

public class GridTileView : MonoBehaviour
{
    
    public Vector2Int GridPosition;
    public float objectYOffset = 0.8f;
    [Header("Kilit Ayarları")]
    public bool StartLocked = false;//bunu işaretlersen kilitli başlar
    public MeshRenderer MyMeshRenderer; //renk burdan değişecek
    public Material LockedMaterial;
    public Material NormalMaterial;


    void Start()
    {
        GridManager gridManager = GridManager.Instance;

        if (gridManager == null)
        {
            Debug.LogError("Sahnede GridManager bulunamadı!");
            return;
        }
        if (!gridManager.IsValidPosition(GridPosition))
        {
            Debug.LogError($"GridPosition gecersiz: {GridPosition}");
            return;
        }

      
        //GridPosition = new Vector2Int(
        //    Mathf.RoundToInt(transform.position.x),
        //    Mathf.RoundToInt(transform.position.z)
        //);

       
        gridManager.RegisterTile(this, GridPosition);
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
            transform.position.y + objectYOffset, // Tile'ın Y'sine offset ekle
            transform.position.z
        );
    }

    public void UpdateVisuals(bool isLocked)
    {
        if(MyMeshRenderer != null && LockedMaterial != null && NormalMaterial != null)
        {
            MyMeshRenderer.material = isLocked ? LockedMaterial : NormalMaterial;
        }
    }
}
