using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string baseSceneName = "BaseScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        Time.timeScale = 1f;

        ShowMainPanel();
    }

    public void OnStartButtonClicked()
    {
      
        if (CinematicPlayer.Instance != null)
        {
            CinematicPlayer.Instance.PlayIntroThenLoadScene(baseSceneName);
            return;
        }

          SceneManager.LoadScene(baseSceneName);
    }

    public void OnSettingsButtonClicked()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

    }

    public void OnSettingsBackButtonClicked()
    {
        ShowMainPanel();
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Oyundan çıkılıyor...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMainPanel()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}