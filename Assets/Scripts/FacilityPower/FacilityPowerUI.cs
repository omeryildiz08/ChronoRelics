using System.Collections;
using TMPro;
using UnityEngine;

public class FacilityPowerUI : MonoBehaviour
{
    public TextMeshProUGUI facilityPowerText;
    public string prefix = "FP: ";
    public bool showUnlockReadyCount = false;
    public string readySuffix = " | Ready: ";

    private FacilityPowerManager facilityPowerManager;

    private IEnumerator Start()
    {
        while (FacilityPowerManager.Instance == null)
        {
            yield return null;
        }

        facilityPowerManager = FacilityPowerManager.Instance;
        facilityPowerManager.OnFacilityPowerChanged += UpdateText;
        UpdateText(
            facilityPowerManager.CurrentFacilityPower,
            facilityPowerManager.UsedFacilityPower,
            facilityPowerManager.AvailableFacilityPower,
            0,
            false,
            0);
    }

    private void OnDestroy()
    {
        if (facilityPowerManager != null)
        {
            facilityPowerManager.OnFacilityPowerChanged -= UpdateText;
        }
    }

    private void UpdateText(
        int currentPower,
        int usedPower,
        int availablePower,
        int nextRequiredPower,
        bool hasNextThreshold,
        int unlockableTileCount)
    {
        if (facilityPowerText == null)
        {
            return;
        }

        if (unlockableTileCount > 0)
        {
            facilityPowerText.text = showUnlockReadyCount
                ? $"{prefix}{availablePower}{readySuffix}{unlockableTileCount}"
                : $"{prefix}{availablePower}";
            return;
        }

        facilityPowerText.text = $"{prefix}{availablePower}";
    }
}
