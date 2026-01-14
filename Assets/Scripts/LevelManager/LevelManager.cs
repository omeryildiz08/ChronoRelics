using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
   [Header("Level Hedefleri")]
   public MergeableItemData TargetItem;
   public MergeableItemData RewardItem; //kazanılacak(base'e gidecek)ödül

   [Header("UI Referansları")]
   public GameObject WinPanel; 

   [Header("Sahne Ayarları")]
   public string BaseSceneName ="BaseScene";
   [Header("Audio Settings")]
    public AudioSource audioSource;       
    public AudioClip levelCompleteSound;


   private bool isLevelCompleted = false;

    void Start()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted += OnMergeHappened;
        }
    }

    void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnMergeCompleted -= OnMergeHappened;
        }
    }

    private void OnMergeHappened(MergeableItemData producedItem)
    {
        if(isLevelCompleted) return; 

        if(producedItem == TargetItem)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {

        isLevelCompleted = true;
        Debug.Log("Seviye Tamamlandı!");
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
        
        if(SaveManager.Instance != null && RewardItem != null)
        {
            SaveManager.Instance.AddRewardToInventory(RewardItem.ItemID);
        } 

        if (WinPanel != null)
        {
            WinPanel.SetActive(true);
            //efektler gelebilir(konfeti vb ses)
        }
    }

    public void LoadBaseScene()
    {
        if(SaveManager.Instance != null)
        {
            SceneManager.LoadScene(BaseSceneName);
        }
    }


}
