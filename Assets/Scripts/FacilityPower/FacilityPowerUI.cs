using TMPro;
using UnityEngine;

public class FacilityPowerUI : MonoBehaviour
{
    public TextMeshProUGUI facilityPowerText;
    public string prefix = "Facility Power: ";
    public string readySuffix = " | Unlock Ready";

    private FacilityPowerManager facilityPowerManager;

    private void Start()
    {
        facilityPowerManager = FacilityPowerManager.Instance;
        if (facilityPowerManager != null)
        {
            facilityPowerManager.OnFacilityPowerChanged += UpdateText;
            UpdateText(
                facilityPowerManager.CurrentFacilityPower,
                0,
                false,
                0);
        }
    }

    private void OnDestroy()
    {
        if (facilityPowerManager != null)
        {
            facilityPowerManager.OnFacilityPowerChanged -= UpdateText;
        }
    }

    private void UpdateText(int currentPower, int nextRequiredPower, bool hasNextThreshold, int unlockableTileCount)
    {
        if (facilityPowerText == null)
        {
            return;
        }

        if (unlockableTileCount > 0)
        {
            facilityPowerText.text = $"{prefix}{currentPower}{readySuffix}: {unlockableTileCount}";
            return;
        }

        if (hasNextThreshold)
        {
            facilityPowerText.text = $"{prefix}{currentPower}/{nextRequiredPower}";
            return;
        }

        facilityPowerText.text = $"{prefix}{currentPower}";
    }
}
