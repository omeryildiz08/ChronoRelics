using System;
using System.Collections;
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
    public Material UnlockableMaterial;

    [Header("Unlock Animation")]
    [Min(0.01f)] public float unlockAnimationSeconds = 0.45f;
    [Range(0.5f, 1f)] public float unlockShrinkScaleMultiplier = 0.72f;
    [Header("Drag Highlight")]
    [SerializeField] private GameObject highlightOverlay;
    [SerializeField] private MeshRenderer highlightRenderer;
    [SerializeField] private Material originHighlightMaterial;
    [SerializeField] private Material validHighlightMaterial;
    [SerializeField] private Material mergeCandidateHighlightMaterial;
    [SerializeField] private Material invalidHighlightMaterial;

    private bool isRegistered;
    private Coroutine unlockAnimationRoutine;

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

    public void SetUnlockableVisual(bool isUnlockable)
    {
        if (isUnlockable && MyMeshRenderer != null && UnlockableMaterial != null)
        {
            MyMeshRenderer.material = UnlockableMaterial;
            return;
        }

        GridManager gridManager = GridManager.Instance;
        bool isLocked = gridManager != null
            ? gridManager.IsTileLocked(GridPosition)
            : StartLocked;

        UpdateVisuals(isLocked);
    }

    public void PlayUnlockAnimation(Action onComplete)
    {
        if (unlockAnimationRoutine != null)
        {
            StopCoroutine(unlockAnimationRoutine);
        }

        unlockAnimationRoutine = StartCoroutine(UnlockAnimationRoutine(onComplete));
    }

    private IEnumerator UnlockAnimationRoutine(Action onComplete)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * unlockShrinkScaleMultiplier;
        float halfDuration = Mathf.Max(0.01f, unlockAnimationSeconds * 0.5f);

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        unlockAnimationRoutine = null;
        onComplete?.Invoke();
    }

    private void OnMouseDown()
    {
        if (UIInputBlocker.IsPointerOverUI())
        {
            return;
        }

        if (FacilityPowerManager.Instance != null)
        {
            FacilityPowerManager.Instance.TryUnlockTile(this);
        }
    }

    public void SetHighlight(TileHighlightState state)
    {
        if (highlightOverlay == null)
        {
            return;
        }

        if (state == TileHighlightState.None)
        {
            highlightOverlay.SetActive(false);
            return;
        }

        highlightOverlay.SetActive(true);

        if (highlightRenderer == null)
        {
            return;
        }

        Material targetMaterial = GetHighlightMaterial(state);

        if (targetMaterial != null)
        {
            highlightRenderer.material = targetMaterial;
        }
    }

    private Material GetHighlightMaterial(TileHighlightState state)
    {
        switch (state)
        {
            case TileHighlightState.Origin:
                return originHighlightMaterial;

            case TileHighlightState.Valid:
                return validHighlightMaterial;

            case TileHighlightState.MergeCandidate:
                return mergeCandidateHighlightMaterial;

            case TileHighlightState.Invalid:
                return invalidHighlightMaterial;

            default:
                return null;
        }
    }

    public void ClearHighlight()
    {
        SetHighlight(TileHighlightState.None);
    }
}

public enum TileHighlightState
{
    None,
    Origin,
    Valid,
    MergeCandidate,
    Invalid
}
