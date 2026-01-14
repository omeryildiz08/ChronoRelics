using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUI_Item : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Image IconImage;          
    public GameObject DescriptionPanel; // Açılan balon (Card)
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ProgressText; 
    public GameObject Checkmark;      
    private LevelQuestData myQuestData;
    private int currentAmount;
    private bool isCompleted = false;
    [Header("Sfx Referansları")]
    public AudioClip QuestTabSound;
    public AudioSource audioSource;

    public void Setup(LevelQuestData data)
    {
        myQuestData = data;
        IconImage.sprite = data.Icon;
        DescriptionText.text = data.Description;
        currentAmount = 0;

        UpdateProgressText();
        DescriptionPanel.SetActive(false); // Başta kapalı olsun
        Checkmark.SetActive(false);
    }

    public void ToggleDescription()
    {
        if (audioSource != null && QuestTabSound != null)
        {
            audioSource.PlayOneShot(QuestTabSound);
        }
        bool isActive = DescriptionPanel.activeSelf;
        DescriptionPanel.SetActive(!isActive);
        Debug.Log("görev tab açıldı");
    }

    public void AddProgress(int amount)
    {
        if (isCompleted) return;

        currentAmount += amount;
        Debug.Log($"Görev ilerlemesi: {currentAmount}/{myQuestData.RequiredAmount}");
        UpdateProgressText();

        if (currentAmount >= myQuestData.RequiredAmount)
        {
            Debug.Log("Görev tamamlandı");
            CompleteQuest();
        }
    }

    void UpdateProgressText()
    {
        ProgressText.text = $"{currentAmount}/{myQuestData.RequiredAmount}";
    }

    void CompleteQuest()
    {
        isCompleted = true;
        Checkmark.SetActive(true);
        ProgressText.text = "TAMAMLANDI";
        // Burada bir ses veya efekt çalabilir
    }

    public LevelQuestData GetQuestData() { return myQuestData; }
}
