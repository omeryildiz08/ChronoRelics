using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Panels")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        ForceResume();

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                OnContinueButtonClicked();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OnContinueButtonClicked()
    {
        ForceResume();

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(false);
        }
    }

    public void OnSettingsButtonClicked()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OnSettingsBackButtonClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void OnMainMenuButtonClicked()
    {
        ForceResume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButtonClicked()
    {
        ForceResume();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ForceResume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }
}