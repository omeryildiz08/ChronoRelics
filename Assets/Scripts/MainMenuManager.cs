using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string baseSceneName = "BaseScene";
   public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(baseSceneName);
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit();
    }
}
