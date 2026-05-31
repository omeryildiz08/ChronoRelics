using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    private const string MasterVolumeKey = "Settings_MasterVolume";

    private const string FullscreenKey = "Settings_Fullscreen";

    [Header("UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        LoadSettings();
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    private void LoadSettings()
    {
        float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        bool savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(savedMasterVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
        }

        ApplyMasterVolume(savedMasterVolume);
        ApplyFullscreen(savedFullscreen);
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();

        ApplyMasterVolume(value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        ApplyFullscreen(isFullscreen);
    }

    private void ApplyMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
