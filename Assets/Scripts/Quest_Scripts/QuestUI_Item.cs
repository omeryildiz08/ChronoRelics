using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI_Item : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Image IconImage;          
    public GameObject DescriptionPanel; // Açılan balon (Card)
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ProgressText; 
    public GameObject Checkmark;      
    private LevelQuestData myQuestData;
    private int currentAmount;
    private bool isCompleted = false;
    [Header("Sfx Referansları")]
    public AudioClip QuestTabSound;
    public AudioSource audioSource;

    [Header("Acilma Animasyonu")]
    public bool UseAnimatedDescription = true;
    public bool PositionDescriptionRightOfIcon = true;
    public bool UseOpenPositionOverride = true;
    public Vector2 OpenPositionOverride = new Vector2(260f, 0f);
    [Min(0.01f)] public float OpenDuration = 0.25f;
    [Min(0.01f)] public float CloseDuration = 0.18f;
    public float IconToDescriptionSpacing = 8f;
    public Vector2 ClosedOffset = new Vector2(-120f, 0f);
    public Vector3 ClosedScale = new Vector3(0.65f, 0.9f, 1f);
    public Ease OpenEase = Ease.OutBack;
    public Ease CloseEase = Ease.InCubic;

    private RectTransform descriptionRect;
    private CanvasGroup descriptionCanvasGroup;
    private Vector2 openAnchoredPosition;
    private Vector3 openScale;
    private Tween descriptionTween;

    public void Setup(LevelQuestData data)
    {
        myQuestData = data;
        IconImage.sprite = data.Icon;
        DescriptionText.text = data.Description;
        currentAmount = 0;

        UpdateProgressText();
        CacheDescriptionAnimationState();
        SetDescriptionInstant(false);
        Checkmark.SetActive(false);
    }

    public void ToggleDescription()
    {
        if (audioSource != null && QuestTabSound != null)
        {
            audioSource.PlayOneShot(QuestTabSound);
        }
        bool shouldOpen = !DescriptionPanel.activeSelf;
        SetDescriptionVisible(shouldOpen);
        Debug.Log("görev tab açıldı");
    }

    public void AddProgress(int amount)
    {
        if (isCompleted) return;

        currentAmount += amount;
        Debug.Log($"Görev ilerlemesi: {currentAmount}/{myQuestData.RequiredAmount}");
        UpdateProgressText();

        if (currentAmount >= myQuestData.RequiredAmount)
        {
            Debug.Log("Görev tamamlandı");
            CompleteQuest();
        }
    }

    void UpdateProgressText()
    {
        ProgressText.text = $"{currentAmount}/{myQuestData.RequiredAmount}";
    }

    void CompleteQuest()
    {
        isCompleted = true;
        Checkmark.SetActive(true);
        ProgressText.text = "TAMAMLANDI";
        // Burada bir ses veya efekt çalabilir
    }

    public LevelQuestData GetQuestData() { return myQuestData; }

    private void CacheDescriptionAnimationState()
    {
        if (DescriptionPanel == null)
        {
            return;
        }

        descriptionRect = DescriptionPanel.GetComponent<RectTransform>();
        descriptionCanvasGroup = DescriptionPanel.GetComponent<CanvasGroup>();

        if (descriptionCanvasGroup == null)
        {
            descriptionCanvasGroup = DescriptionPanel.AddComponent<CanvasGroup>();
        }

        LayoutElement layoutElement = DescriptionPanel.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = DescriptionPanel.AddComponent<LayoutElement>();
        }

        layoutElement.ignoreLayout = true;

        if (descriptionRect != null)
        {
            openAnchoredPosition = GetOpenAnchoredPosition();
            openScale = descriptionRect.localScale;
        }
    }

    private Vector2 GetOpenAnchoredPosition()
    {
        Vector2 fallbackPosition = descriptionRect.anchoredPosition;
        if (UseOpenPositionOverride)
        {
            return OpenPositionOverride;
        }

        if (!PositionDescriptionRightOfIcon || IconImage == null)
        {
            return fallbackPosition;
        }

        RectTransform iconRect = IconImage.rectTransform;
        if (iconRect == null || iconRect.parent != descriptionRect.parent)
        {
            return fallbackPosition;
        }

        float iconRightEdge = iconRect.anchoredPosition.x + iconRect.rect.width * (1f - iconRect.pivot.x);
        float descriptionPivotOffset = descriptionRect.rect.width * descriptionRect.pivot.x;
        float openX = iconRightEdge + descriptionPivotOffset + IconToDescriptionSpacing;

        if (openX < descriptionPivotOffset)
        {
            openX = descriptionPivotOffset + IconToDescriptionSpacing;
        }

        return new Vector2(
            openX,
            iconRect.anchoredPosition.y
        );
    }

    private void SetDescriptionVisible(bool shouldOpen)
    {
        if (DescriptionPanel == null)
        {
            return;
        }

        if (!UseAnimatedDescription || descriptionRect == null || descriptionCanvasGroup == null)
        {
            DescriptionPanel.SetActive(shouldOpen);
            return;
        }

        descriptionTween?.Kill();

        if (shouldOpen)
        {
            DescriptionPanel.SetActive(true);
            descriptionCanvasGroup.blocksRaycasts = true;
            descriptionCanvasGroup.interactable = true;
            descriptionCanvasGroup.alpha = 0f;
            descriptionRect.anchoredPosition = openAnchoredPosition + ClosedOffset;
            descriptionRect.localScale = ClosedScale;

            Sequence sequence = DOTween.Sequence();
            sequence.Join(descriptionCanvasGroup.DOFade(1f, OpenDuration));
            sequence.Join(descriptionRect.DOAnchorPos(openAnchoredPosition, OpenDuration).SetEase(Ease.OutCubic));
            sequence.Join(descriptionRect.DOScale(openScale, OpenDuration).SetEase(OpenEase));
            descriptionTween = sequence;
            return;
        }

        descriptionCanvasGroup.blocksRaycasts = false;
        descriptionCanvasGroup.interactable = false;

        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Join(descriptionCanvasGroup.DOFade(0f, CloseDuration));
        closeSequence.Join(descriptionRect.DOAnchorPos(openAnchoredPosition + ClosedOffset, CloseDuration).SetEase(CloseEase));
        closeSequence.Join(descriptionRect.DOScale(ClosedScale, CloseDuration).SetEase(CloseEase));
        closeSequence.OnComplete(() => DescriptionPanel.SetActive(false));
        descriptionTween = closeSequence;
    }

    private void SetDescriptionInstant(bool isOpen)
    {
        if (DescriptionPanel == null)
        {
            return;
        }

        if (descriptionTween != null)
        {
            descriptionTween.Kill();
            descriptionTween = null;
        }

        DescriptionPanel.SetActive(isOpen);

        if (descriptionCanvasGroup != null)
        {
            descriptionCanvasGroup.alpha = isOpen ? 1f : 0f;
            descriptionCanvasGroup.blocksRaycasts = isOpen;
            descriptionCanvasGroup.interactable = isOpen;
        }

        if (descriptionRect != null)
        {
            descriptionRect.anchoredPosition = isOpen ? openAnchoredPosition : openAnchoredPosition + ClosedOffset;
            descriptionRect.localScale = isOpen ? openScale : ClosedScale;
        }
    }

    private void OnDestroy()
    {
        descriptionTween?.Kill();
    }
}
