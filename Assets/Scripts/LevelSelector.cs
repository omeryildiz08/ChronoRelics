using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [System.Serializable]
    public class LevelChronoCost
    {
        public string LevelSceneName;
        [Min(0)] public int ChronoCost = 1;
    }

    [Header("UI Referanslari")]
    public GameObject selectionPanel;
    public TextMeshProUGUI feedbackText;

    [Header("Feedback")]
    [Min(0f)] public float feedbackVisibleSeconds = 1f;
    [Min(0f)] public float feedbackFadeSeconds = 0.5f;
    public bool hideFeedbackObjectAfterFade = true;

    [Header("Chrono Charge")]
    public bool requireChronoCharge = true;
    [Min(0)] public int defaultChronoCost = 1;
    public List<LevelChronoCost> levelChronoCosts = new List<LevelChronoCost>();

    private Coroutine feedbackRoutine;

    public void TogglePanel()
    {
        if (selectionPanel != null)
        {
            bool isActive = selectionPanel.activeSelf;
            selectionPanel.SetActive(!isActive);
        }
    }

    public void LoadLevelScene(string levelName)
    {
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
