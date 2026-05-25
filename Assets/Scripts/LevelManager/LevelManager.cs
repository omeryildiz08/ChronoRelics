using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Hedefleri")]
    public MergeableItemData TargetItem;
    public MergeableItemData RewardItem;
    [Min(0)] public int ChronoChargeReward = 0;

    [Header("UI Referanslari")]
    public GameObject WinPanel;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    [Header("Sahne Ayarlari")]
    public string BaseSceneName = "BaseScene";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip levelCompleteSound;

    private bool isLevelCompleted = false;
    public bool IsLevelCompleted => isLevelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Birden fazla LevelManager Instance bulundu. Aktif Instance son Awake olan LevelManager olarak guncellendi.");
        }

        Instance = this;
    }

    private void Start()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted += OnMergeHappened;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

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

        LevelCompleteUI activeLevelCompleteUI = ResolveLevelCompleteUI();
        if (activeLevelCompleteUI != null)
        {
            activeLevelCompleteUI.Show(TargetItem, RewardItem, ChronoChargeReward, LoadBaseScene);
        }
        else if (WinPanel != null)
        {
            WinPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Level tamamlandi ancak LevelCompleteUI veya WinPanel atanmamis.");
        }
    }

    private LevelCompleteUI ResolveLevelCompleteUI()
    {
        if (levelCompleteUI != null)
        {
            return levelCompleteUI;
        }

        return WinPanel != null ? WinPanel.GetComponent<LevelCompleteUI>() : null;
    }

    public void LoadBaseScene()
    {
        if (SaveManager.Instance != null)
        {
            SceneManager.LoadScene(BaseSceneName);
        }
    }
}
