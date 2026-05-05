using TMPro;
using UnityEngine;

public class ChronoChargeUI : MonoBehaviour
{
    public TextMeshProUGUI chronoChargeText;
    public string prefix = "";

    private void Update()
    {
        if (chronoChargeText == null || SaveManager.Instance == null)
        {
            return;
        }

        chronoChargeText.text = $"{prefix}{SaveManager.Instance.CurrentChronoCharge}";
    }
}
