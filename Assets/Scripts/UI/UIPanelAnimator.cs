using System;
using DG.Tweening;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("References")]
    public RectTransform panelRect;
    public RectTransform sourceButton;

    [Header("Closed Position")]
    public bool useSourceButtonAsClosedPosition;
    public Vector2 closedOffset = new Vector2(0f, -220f);

    [Header("Timing")]
    [Min(0f)] public float openDuration = 0.25f;
    [Min(0f)] public float closeDuration = 0.18f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InCubic;
    public bool useUnscaledTime = true;

    [Header("Scale & Fade")]
    public Vector3 closedScaleMultiplier = new Vector3(0.7f, 0.7f, 1f);
    public bool animateFade = true;

    private CanvasGroup canvasGroup;
    private Sequence activeSequence;
    private Vector2 openAnchoredPosition;
    private Vector3 openScale;
    private bool initialized;
    private bool targetVisible;

    public bool IsVisible => targetVisible;

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        KillActiveSequence();
    }

    public void Show(Action onComplete = null)
    {
        Initialize();

        if (panelRect == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillActiveSequence();
        targetVisible = true;

        GameObject panelObject = panelRect.gameObject;
        if (!panelObject.activeSelf)
        {
            panelObject.SetActive(true);
        }

        SetInteractable(false);

        panelRect.anchoredPosition = GetClosedAnchoredPosition();
        panelRect.localScale = GetClosedScale();

        if (animateFade)
        {
            canvasGroup.alpha = 0f;
        }

        activeSequence = DOTween.Sequence();
        activeSequence.SetUpdate(useUnscaledTime);
        activeSequence.Join(panelRect.DOAnchorPos(openAnchoredPosition, openDuration).SetEase(openEase));
        activeSequence.Join(panelRect.DOScale(openScale, openDuration).SetEase(openEase));

        if (animateFade)
        {
            activeSequence.Join(canvasGroup.DOFade(1f, Mathf.Min(openDuration, openDuration * 0.75f)).SetEase(Ease.OutQuad));
        }

        activeSequence.OnComplete(() =>
        {
            activeSequence = null;
            panelRect.anchoredPosition = openAnchoredPosition;
            panelRect.localScale = openScale;
            if (animateFade)
            {
                canvasGroup.alpha = 1f;
            }

            SetInteractable(true);
            onComplete?.Invoke();
        });
    }

    public void Hide(Action onComplete = null)
    {
        Initialize();

        if (panelRect == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillActiveSequence();
        targetVisible = false;
        SetInteractable(false);

        if (!panelRect.gameObject.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        activeSequence = DOTween.Sequence();
        activeSequence.SetUpdate(useUnscaledTime);
        activeSequence.Join(panelRect.DOAnchorPos(GetClosedAnchoredPosition(), closeDuration).SetEase(closeEase));
        activeSequence.Join(panelRect.DOScale(GetClosedScale(), closeDuration).SetEase(closeEase));

        if (animateFade)
        {
            activeSequence.Join(canvasGroup.DOFade(0f, closeDuration).SetEase(Ease.InQuad));
        }

        activeSequence.OnComplete(() =>
        {
            activeSequence = null;
            panelRect.gameObject.SetActive(false);
            panelRect.anchoredPosition = openAnchoredPosition;
            panelRect.localScale = openScale;
            if (animateFade)
            {
                canvasGroup.alpha = 1f;
            }

            onComplete?.Invoke();
        });
    }

    public void Toggle(Action onShown = null, Action onHidden = null)
    {
        if (targetVisible)
        {
            Hide(onHidden);
        }
        else
        {
            Show(onShown);
        }
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        if (panelRect == null)
        {
            panelRect = GetComponent<RectTransform>();
        }

        if (panelRect != null)
        {
            openAnchoredPosition = panelRect.anchoredPosition;
            openScale = panelRect.localScale;
            canvasGroup = panelRect.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panelRect.gameObject.AddComponent<CanvasGroup>();
            }

            targetVisible = panelRect.gameObject.activeSelf;
            SetInteractable(targetVisible);
        }

        initialized = true;
    }

    private Vector2 GetClosedAnchoredPosition()
    {
        if (useSourceButtonAsClosedPosition && sourceButton != null && panelRect != null && panelRect.parent is RectTransform parentRect)
        {
            Canvas canvas = panelRect.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            Vector3[] corners = new Vector3[4];
            sourceButton.GetWorldCorners(corners);
            Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldCenter);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, camera, out Vector2 localPoint))
            {
                return localPoint + closedOffset;
            }
        }

        return openAnchoredPosition + closedOffset;
    }

    private Vector3 GetClosedScale()
    {
        return new Vector3(
            openScale.x * closedScaleMultiplier.x,
            openScale.y * closedScaleMultiplier.y,
            openScale.z * closedScaleMultiplier.z
        );
    }

    private void SetInteractable(bool interactable)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    private void KillActiveSequence()
    {
        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
