using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
   [Header("UI Referansları")]
    public GameObject selectionPanel;

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
        Debug.Log($"Level Yükleniyor: {levelName}");

        
        //yüklemeden önce base sahneyi kaydet
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        
        SceneManager.LoadScene(levelName);
    }
}
