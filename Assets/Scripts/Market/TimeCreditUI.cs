using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeCreditUI : MonoBehaviour
{
    public TextMeshProUGUI timeCreditText;
    void Update()
    {
        if(timeCreditText== null || SaveManager.Instance == null) return;
       timeCreditText.text = SaveManager.Instance.CurrentTimeCredits.ToString();


    }
}
