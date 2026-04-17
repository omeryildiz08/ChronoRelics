using Unity.VisualScripting;
using UnityEngine;

public class MergeableObject : MonoBehaviour
{
    public MergeableItemData ItemData;
    public bool IsInactiveAnomalyItem = false;

    public Vector2Int CurrentGridPosition;

    private GridManager gridManager;
    private Camera mainCamera;
    private bool isDragging = false;

    // Mouse ile sürüklerken objenin yerden ne kadar "havalanacağını" belirler.
    private float dragYOffset = 0.8f;

    // Mouse'un pozisyonunu 3D dünyaya çevirirken kullanacağımız sanal düzlem.
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

    private bool isInitialized = false;

    void Start()
    {
        if(isInitialized) return;
     
        InitializeObject();

        gridManager = GridManager.Instance;
        mainCamera = Camera.main;
        if (gridManager == null || mainCamera == null)
        {
            Debug.LogError("MergeableObject missing GridManager or Camera.main");
            enabled = false;
            return;
        }

        
        CurrentGridPosition = gridManager.WorldToGridPosition(transform.position);

        
        gridManager.RegisterObject(this, CurrentGridPosition);
    }

    private void OnMouseDown()
    {
        if (IsInactiveAnomalyItem)
        {
            gridManager.PlayErrorSound();
            return;
        }

        if (gridManager.IsTileLocked(CurrentGridPosition))
        {
            Debug.Log("Bu obje kilitli sürüklenemez");
            gridManager.PlayErrorSound();
            return;
            //buraya animasyon veya ses gelebilir
        }


        if (MarketManager.Instance != null)
        {
            MarketManager.Instance.SetSellZoneActive(true);
        }

        isDragging = true;
        
        transform.position += new Vector3(0, dragYOffset, 0);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            
            transform.position = new Vector3(worldPosition.x, dragYOffset, worldPosition.z);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if(MarketManager.Instance != null)
        {
            MarketManager.Instance.SetSellZoneActive(false);

            if(MarketManager.Instance.sellZoneRect!=null)
            {
                bool isDroppedInSellZone = RectTransformUtility.RectangleContainsScreenPoint(MarketManager.Instance.sellZoneRect, Input.mousePosition, null);

                if (isDroppedInSellZone)
                {
                    MarketManager.Instance.TrySellObject(this);
                    return; // Satıldıysa normal bırakma işlemi yapılmaz
                }
                
            }
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPositionOnDrop = transform.position; // Default
        if (groundPlane.Raycast(ray, out float distance))
        {
            worldPositionOnDrop = ray.GetPoint(distance);
        }

        
        Vector2Int toPos = gridManager.WorldToGridPosition(worldPositionOnDrop);

        //GridManager'a "Ben bu objeyi 'CurrentGridPosition'dan
        // 'toPos'a bırakıyorum, birleşme mi olacak, taşıma mı, karar ver" de.
        gridManager.TryMergeOrPlace(this, CurrentGridPosition, toPos);
    }
    
    public void InitializeObject() 
    {
        if (isInitialized) return;
        
        gridManager = GridManager.Instance;
        mainCamera = Camera.main;
        
        // Guard Clauses...
        
        // Grid pozisyonunu bul ve kaydet
        if (CurrentGridPosition == Vector2Int.zero) // SaveManager set etmediyse hesapla
        {
             CurrentGridPosition = gridManager.WorldToGridPosition(transform.position);
        }

        gridManager.RegisterObject(this, CurrentGridPosition);
        isInitialized = true;
    }
}
