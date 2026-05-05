using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Hedefleri")]
    public MergeableItemData TargetItem;
    public MergeableItemData RewardItem;
    [Min(0)] public int ChronoChargeReward = 0;

    [Header("UI Referanslari")]
    public GameObject WinPanel;

    [Header("Sahne Ayarlari")]
    public string BaseSceneName = "BaseScene";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip levelCompleteSound;

    private bool isLevelCompleted = false;

    private void Start()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted += OnMergeHappened;
        }
    }

    private void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted -= OnMergeHappened;
        }
    }

    private void OnMergeHappened(MergeableItemData producedItem)
    {
        if (isLevelCompleted)
        {
            return;
        }

        if (producedItem == TargetItem)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        isLevelCompleted = true;
        Debug.Log("Seviye tamamlandi.");

        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        if (SaveManager.Instance != null && RewardItem != null)
        {
            SaveManager.Instance.AddRewardToInventory(RewardItem.ItemID);
        }

        if (SaveManager.Instance != null && ChronoChargeReward > 0)
        {
            SaveManager.Instance.AddChronoCharge(ChronoChargeReward);
        }

        if (WinPanel != null)
        {
            WinPanel.SetActive(true);
        }
    }

    public void LoadBaseScene()
    {
        if (SaveManager.Instance != null)
        {
            SceneManager.LoadScene(BaseSceneName);
        }
    }
}
