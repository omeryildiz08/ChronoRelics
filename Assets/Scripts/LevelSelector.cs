using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [System.Serializable]
    public class LevelChronoCost
    {
        public string LevelSceneName;
        [Min(0)] public int ChronoCost = 1;
    }

    [System.Serializable]
    public class TimePeriodLevelGroup
    {
        public string PeriodId;
        public string DisplayName;
        public GameObject LevelGroup;
        public Button SelectButton;
        public GameObject SelectedIndicator;
        public Color AccentColor = Color.white;
    }

    [Header("UI Referanslari")]
    public GameObject selectionPanel;
    public TextMeshProUGUI feedbackText;

    [Header("Zaman Dilimi Secimi")]
    public List<TimePeriodLevelGroup> timePeriodGroups = new List<TimePeriodLevelGroup>();
    [Min(0)] public int defaultTimePeriodIndex = 0;
    public bool showDefaultTimePeriodOnPanelOpen = true;
    public bool hideLevelGroupsOnPanelClose = true;

    [Header("Secili Zaman Gorsel Geri Bildirimi")]
    public TextMeshProUGUI selectedPeriodTitle;
    public List<Graphic> periodAccentGraphics = new List<Graphic>();
    public bool tintSelectedButton = true;
    public bool scaleSelectedButton = true;
    [Min(1f)] public float selectedButtonScaleMultiplier = 1.08f;
    public bool logTimePeriodSelection = true;
    public bool bindTimePeriodButtonsOnAwake = true;
    public bool keepTimePeriodButtonsInFront = true;

    [Header("Feedback")]
    [Min(0f)] public float feedbackVisibleSeconds = 1f;
    [Min(0f)] public float feedbackFadeSeconds = 0.5f;
    public bool hideFeedbackObjectAfterFade = true;

    [Header("Chrono Charge")]
    public bool requireChronoCharge = true;
    [Min(0)] public int defaultChronoCost = 1;
    public List<LevelChronoCost> levelChronoCosts = new List<LevelChronoCost>();

    private Coroutine feedbackRoutine;
    private int activeTimePeriodIndex = -1;
    private readonly Dictionary<Button, ButtonVisualState> buttonVisualStates = new Dictionary<Button, ButtonVisualState>();

    private struct ButtonVisualState
    {
        public Vector3 Scale;
        public Color GraphicColor;
    }

    private void Awake()
    {
        CacheButtonVisualStates();
        BindTimePeriodButtons();
        HideAllTimePeriodGroups();
    }

    private void Start()
    {
        if (selectionPanel != null && selectionPanel.activeSelf && showDefaultTimePeriodOnPanelOpen && activeTimePeriodIndex < 0)
        {
            SelectDefaultTimePeriod();
        }

        BringTimePeriodButtonsToFront();
    }

    public void TogglePanel()
    {
        if (selectionPanel != null)
        {
            bool isActive = selectionPanel.activeSelf;
            bool shouldShow = !isActive;
            selectionPanel.SetActive(shouldShow);

            if (shouldShow)
            {
                if (showDefaultTimePeriodOnPanelOpen && activeTimePeriodIndex < 0)
                {
                    SelectDefaultTimePeriod();
                }

                BringTimePeriodButtonsToFront();
            }
            else if (hideLevelGroupsOnPanelClose)
            {
                HideAllTimePeriodGroups();
            }
        }
    }

    public void SelectDefaultTimePeriod()
    {
        SelectTimePeriodByIndex(defaultTimePeriodIndex);
    }

    public void SelectTimePeriodByIndex(int index)
    {
        if (index < 0 || index >= timePeriodGroups.Count)
        {
            SetFeedback($"Zaman secimi bulunamadi. Index: {index}");
            return;
        }

        SetActiveTimePeriod(index);
    }

    public void SelectTimePeriod(string periodId)
    {
        if (string.IsNullOrWhiteSpace(periodId))
        {
            SetFeedback("Zaman dilimi adi bos.");
            return;
        }

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group == null)
            {
                continue;
            }

            if (string.Equals(group.PeriodId, periodId, StringComparison.OrdinalIgnoreCase))
            {
                SetActiveTimePeriod(i);
                return;
            }
        }

        SetFeedback($"Zaman dilimi bulunamadi: {periodId}");
    }

    public void LoadLevelScene(string levelName)
    {
        if (logTimePeriodSelection)
        {
            Debug.Log($"[LevelSelector] LoadLevelScene called. RequestedScene='{levelName}', ActivePeriod='{GetActivePeriodDebugName()}'.");
        }

        if (string.IsNullOrWhiteSpace(levelName))
        {
            SetFeedback("Level adi bos.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(levelName))
        {
            SetFeedback($"Level bulunamadi: {levelName}");
            Debug.LogError($"Level sahnesi Build Settings icinde bulunamadi: {levelName}");
            return;
        }

        int chronoCost = GetChronoCost(levelName);
        if (SaveManager.Instance != null)
        {
            if (requireChronoCharge && !SaveManager.Instance.CanSpendChronoCharge(chronoCost))
            {
                SetFeedback($"Yetersiz Chrono Charge. Gerekli: {chronoCost}");
                return;
            }

            if (requireChronoCharge && !SaveManager.Instance.SpendChronoCharge(chronoCost, false))
            {
                SetFeedback("Chrono Charge harcanamadi.");
                return;
            }

            SaveManager.Instance.SaveGame();
        }

        SetFeedback($"Level yukleniyor: {levelName}");
        Debug.Log($"Level Yukleniyor: {levelName}. ChronoCost={chronoCost}");
        SceneManager.LoadScene(levelName);
    }

    private void SetActiveTimePeriod(int index)
    {
        if (logTimePeriodSelection)
        {
            Debug.Log($"[LevelSelector] SelectTimePeriod index={index}, period='{GetPeriodDebugName(timePeriodGroups[index])}'.");
        }

        if (selectionPanel != null && !selectionPanel.activeSelf)
        {
            selectionPanel.SetActive(true);
        }

        activeTimePeriodIndex = index;

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group == null)
            {
                continue;
            }

            bool isSelected = i == index;
            GameObject levelGroup = GetLevelGroupObject(group);
            GameObject selectedIndicator = group.SelectedIndicator != levelGroup ? group.SelectedIndicator : null;

            if (levelGroup != null)
            {
                levelGroup.SetActive(isSelected);
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(isSelected);
            }

            ApplyButtonVisualState(group, isSelected);

            if (logTimePeriodSelection)
            {
                Debug.Log(
                    $"[LevelSelector] Group '{GetPeriodDebugName(group)}' selected={isSelected}, " +
                    $"LevelGroup='{GetObjectPath(levelGroup)}', LevelGroupActive={(levelGroup != null && levelGroup.activeInHierarchy)}, " +
                    $"ConfiguredLevelGroup='{GetObjectPath(group.LevelGroup)}', " +
                    $"SelectedIndicator='{GetObjectPath(group.SelectedIndicator)}', " +
                    $"SelectButton='{GetObjectPath(group.SelectButton != null ? group.SelectButton.gameObject : null)}'."
                );
            }
        }

        ApplySelectedPeriodHeader(timePeriodGroups[index]);
        BringTimePeriodButtonsToFront();
    }

    private void HideAllTimePeriodGroups()
    {
        activeTimePeriodIndex = -1;

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group == null)
            {
                continue;
            }

            GameObject levelGroup = GetLevelGroupObject(group);
            GameObject selectedIndicator = group.SelectedIndicator != levelGroup ? group.SelectedIndicator : null;

            if (levelGroup != null)
            {
                levelGroup.SetActive(false);
            }

            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(false);
            }

            ApplyButtonVisualState(group, false);
        }   

        if (selectedPeriodTitle != null)
        {
            selectedPeriodTitle.text = string.Empty;
        }
    }

    private GameObject GetLevelGroupObject(TimePeriodLevelGroup group)
    {
        if (group == null)
        {
            return null;
        }

        if (group.LevelGroup != null && !IsSelectButtonObject(group, group.LevelGroup))
        {
            return group.LevelGroup;
        }

        if (group.SelectedIndicator != null && !IsSelectButtonObject(group, group.SelectedIndicator))
        {
            return group.SelectedIndicator;
        }

        return group.LevelGroup;
    }

    private string GetActivePeriodDebugName()
    {
        if (activeTimePeriodIndex < 0 || activeTimePeriodIndex >= timePeriodGroups.Count)
        {
            return "None";
        }

        return GetPeriodDebugName(timePeriodGroups[activeTimePeriodIndex]);
    }

    private string GetPeriodDebugName(TimePeriodLevelGroup group)
    {
        if (group == null)
        {
            return "Null";
        }

        if (!string.IsNullOrWhiteSpace(group.PeriodId))
        {
            return group.PeriodId;
        }

        if (!string.IsNullOrWhiteSpace(group.DisplayName))
        {
            return group.DisplayName;
        }

        return "Unnamed";
    }

    private string GetObjectPath(GameObject target)
    {
        if (target == null)
        {
            return "None";
        }

        Transform current = target.transform;
        string path = current.name;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }

    private bool IsSelectButtonObject(TimePeriodLevelGroup group, GameObject candidate)
    {
        if (group == null || candidate == null)
        {
            return false;
        }

        if (group.SelectButton != null && candidate == group.SelectButton.gameObject)
        {
            return true;
        }

        return candidate.GetComponent<Button>() != null && candidate.name.EndsWith("Select", StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySelectedPeriodHeader(TimePeriodLevelGroup group)
    {
        if (group == null)
        {
            return;
        }

        string displayName = string.IsNullOrWhiteSpace(group.DisplayName) ? group.PeriodId : group.DisplayName;

        if (selectedPeriodTitle != null)
        {
            selectedPeriodTitle.text = displayName;
            selectedPeriodTitle.color = group.AccentColor;
        }

        for (int i = 0; i < periodAccentGraphics.Count; i++)
        {
            Graphic accentGraphic = periodAccentGraphics[i];
            if (accentGraphic != null)
            {
                accentGraphic.color = group.AccentColor;
            }
        }
    }

    private void ApplyButtonVisualState(TimePeriodLevelGroup group, bool isSelected)
    {
        if (group == null || group.SelectButton == null)
        {
            return;
        }

        Button button = group.SelectButton;
        ButtonVisualState defaultState;
        if (!buttonVisualStates.TryGetValue(button, out defaultState))
        {
            defaultState = CaptureButtonVisualState(button);
            buttonVisualStates[button] = defaultState;
        }

        if (scaleSelectedButton)
        {
            button.transform.localScale = isSelected
                ? defaultState.Scale * selectedButtonScaleMultiplier
                : defaultState.Scale;
        }

        if (tintSelectedButton && button.targetGraphic != null)
        {
            button.targetGraphic.color = isSelected ? group.AccentColor : defaultState.GraphicColor;
        }
    }

    private void CacheButtonVisualStates()
    {
        buttonVisualStates.Clear();

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group == null || group.SelectButton == null || buttonVisualStates.ContainsKey(group.SelectButton))
            {
                continue;
            }

            buttonVisualStates.Add(group.SelectButton, CaptureButtonVisualState(group.SelectButton));
        }
    }

    private void BindTimePeriodButtons()
    {
        if (!bindTimePeriodButtonsOnAwake)
        {
            return;
        }

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group == null || group.SelectButton == null)
            {
                continue;
            }

            int capturedIndex = i;
            group.SelectButton.onClick.AddListener(() => SelectTimePeriodByIndex(capturedIndex));
        }
    }

    private void BringTimePeriodButtonsToFront()
    {
        if (!keepTimePeriodButtonsInFront)
        {
            return;
        }

        for (int i = 0; i < timePeriodGroups.Count; i++)
        {
            TimePeriodLevelGroup group = timePeriodGroups[i];
            if (group != null && group.SelectButton != null)
            {
                group.SelectButton.transform.SetAsLastSibling();
            }
        }
    }

    private ButtonVisualState CaptureButtonVisualState(Button button)
    {
        ButtonVisualState state = new ButtonVisualState
        {
            Scale = button.transform.localScale,
            GraphicColor = button.targetGraphic != null ? button.targetGraphic.color : Color.white
        };

        return state;
    }

    private int GetChronoCost(string levelName)
    {
        for (int i = 0; i < levelChronoCosts.Count; i++)
        {
            LevelChronoCost entry = levelChronoCosts[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.LevelSceneName))
            {
                continue;
            }

            if (entry.LevelSceneName == levelName)
            {
                return Mathf.Max(0, entry.ChronoCost);
            }
        }

        return Mathf.Max(0, defaultChronoCost);
    }

    private void SetFeedback(string message)
    {
        Debug.Log($"[LevelSelector] {message}");

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.alpha = 1f;

            if (!feedbackText.gameObject.activeSelf)
            {
                feedbackText.gameObject.SetActive(true);
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(FadeFeedbackRoutine());
        }
    }

    private IEnumerator FadeFeedbackRoutine()
    {
        if (feedbackVisibleSeconds > 0f)
        {
            yield return new WaitForSeconds(feedbackVisibleSeconds);
        }

        if (feedbackText == null)
        {
            feedbackRoutine = null;
            yield break;
        }

        if (feedbackFadeSeconds <= 0f)
        {
            feedbackText.alpha = 0f;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < feedbackFadeSeconds)
            {
                elapsed += Time.deltaTime;
                feedbackText.alpha = Mathf.Lerp(1f, 0f, elapsed / feedbackFadeSeconds);
                yield return null;
            }

            feedbackText.alpha = 0f;
        }

        if (hideFeedbackObjectAfterFade && feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        feedbackRoutine = null;
    }
}
