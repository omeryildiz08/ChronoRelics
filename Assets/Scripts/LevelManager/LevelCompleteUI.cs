using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI producedItemNameText;
    [SerializeField] private TextMeshProUGUI rewardItemNameText;
    [SerializeField] private TextMeshProUGUI chronoChargeRewardText;
    [SerializeField] private Image producedItemIconImage;
    [SerializeField] private Image rewardItemIconImage;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private bool createMissingTextFields = false;

    private Coroutine fadeCoroutine;
    private Transform runtimeSummaryRoot;

    public void Show(
        MergeableItemData producedItem,
        MergeableItemData rewardItem,
        int chronoChargeReward,
        Action onContinue)
    {
        GameObject targetRoot = rootPanel != null ? rootPanel : gameObject;
        targetRoot.SetActive(true);
        EnsureRuntimeReferences(targetRoot);

        if (titleText != null)
        {
            titleText.text = "SEVİYE TAMAMLANDI!";
        }

        if (producedItemNameText != null)
        {
            producedItemNameText.text = $"Produced Item: {GetItemDisplayName(producedItem)}";
        }

        if (rewardItemNameText != null)
        {
            rewardItemNameText.text = $"Reward Item: {GetItemDisplayName(rewardItem)}";
        }

        if (chronoChargeRewardText != null)
        {
            chronoChargeRewardText.text = chronoChargeReward > 0
                ? $"+{chronoChargeReward} Chrono Charge"
                : "Chrono Charge ödülü yok";
        }

        // MergeableItemData currently has no icon Sprite field.
        // When one is added later, assign it here if these Image references are set.
        if (producedItemIconImage != null)
        {
            producedItemIconImage.gameObject.SetActive(false);
        }

        if (rewardItemIconImage != null)
        {
            rewardItemIconImage.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.onClick = new Button.ButtonClickedEvent();
            continueButton.interactable = true;
            continueButton.onClick.AddListener(() =>
            {
                continueButton.interactable = false;
                onContinue?.Invoke();
            });
        }

        if (audioSource != null && panelOpenSound != null)
        {
            audioSource.PlayOneShot(panelOpenSound);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(PlayOpenAnimation(targetRoot.transform));
    }

    private IEnumerator PlayOpenAnimation(Transform targetTransform)
    {
        const float duration = 0.25f;
        float elapsed = 0f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (targetTransform != null)
        {
            targetTransform.localScale = Vector3.one * 0.92f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = t;
            }

            if (targetTransform != null)
            {
                targetTransform.localScale = Vector3.Lerp(Vector3.one * 0.92f, Vector3.one, t);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (targetTransform != null)
        {
            targetTransform.localScale = Vector3.one;
        }

        fadeCoroutine = null;
    }

    private void EnsureRuntimeReferences(GameObject targetRoot)
    {
        if (targetRoot == null)
        {
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = targetRoot.GetComponent<CanvasGroup>();
        }

        if (continueButton == null)
        {
            continueButton = targetRoot.GetComponentInChildren<Button>(true);
        }

        if (createMissingTextFields &&
            producedItemNameText == null &&
            rewardItemNameText == null &&
            chronoChargeRewardText == null)
        {
            EnsureRuntimeSummary(targetRoot.transform);
        }
    }

    private void EnsureRuntimeSummary(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        if (runtimeSummaryRoot == null)
        {
            GameObject summaryObject = new GameObject("Runtime Reward Summary", typeof(RectTransform));
            summaryObject.transform.SetParent(parent, false);
            runtimeSummaryRoot = summaryObject.transform;

            RectTransform rectTransform = summaryObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, -95f);
            rectTransform.sizeDelta = new Vector2(560f, 120f);

            VerticalLayoutGroup layoutGroup = summaryObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 4f;
        }

        if (producedItemNameText == null)
        {
            producedItemNameText = CreateRuntimeText("Produced Item Text", runtimeSummaryRoot);
        }

        if (rewardItemNameText == null)
        {
            rewardItemNameText = CreateRuntimeText("Reward Item Text", runtimeSummaryRoot);
        }

        if (chronoChargeRewardText == null)
        {
            chronoChargeRewardText = CreateRuntimeText("Chrono Charge Text", runtimeSummaryRoot);
        }
    }

    private TextMeshProUGUI CreateRuntimeText(string objectName, Transform parent)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 20f;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        text.enableWordWrapping = false;

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 28f;

        return text;
    }

    private string GetItemDisplayName(MergeableItemData itemData)
    {
        if (itemData == null)
        {
            return "-";
        }

        return string.IsNullOrEmpty(itemData.ItemName) ? itemData.name : itemData.ItemName;
    }
}
