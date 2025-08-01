using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DisplaySettingsController : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle syncToggle;

    private Resolution[] availableResolutions;
    private List<string> resolutionOptions = new List<string>()
    {
        "2560 x 1440",
        "1920 x 1080",
        "1680 x 1050",
        "1080 x 720"
    };
    private List<string> screenModeOptions = new List<string>() {
        "Fullscreen",
        "Windowed"
    };

    void Start()
    {
        // Populate resolution dropdown
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        resolutionOptions.Clear();

        int currentResIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            var res = availableResolutions[i];
            string option = res.width + " x " + res.height;
            resolutionOptions.Add(option);

            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        // Populate screen mode dropdown
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(screenModeOptions);
        screenModeDropdown.value = GetDropdownIndexFromFullScreenMode(Screen.fullScreenMode);
        screenModeDropdown.RefreshShownValue();

        // Populate quality dropdown
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        // Add listeners
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        syncToggle.onValueChanged.AddListener(OnSyncToggleChanged);

        syncToggle.isOn = false; // Manual by default
    }

    void OnResolutionChanged(int index)
    {
        if (!syncToggle.isOn)
        {
            ApplyResolution(index, screenModeDropdown.value);
        }
    }

    void OnScreenModeChanged(int index)
    {
        if (!syncToggle.isOn)
        {
            ApplyResolution(resolutionDropdown.value, index);
        }
    }

    void OnQualityChanged(int index)
    {
        if (!syncToggle.isOn)
        {
            QualitySettings.SetQualityLevel(index);
        }
    }

    void OnSyncToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Apply a default preset when toggled ON (e.g. "High")
            int presetQuality = 4; // "High"
            int presetScreenMode = 0; // Fullscreen Window
            int presetResolutionIndex = availableResolutions.Length - 1; // Highest

            resolutionDropdown.value = presetResolutionIndex;
            screenModeDropdown.value = presetScreenMode;
            qualityDropdown.value = presetQuality;

            ApplyResolution(presetResolutionIndex, presetScreenMode);
            QualitySettings.SetQualityLevel(presetQuality);
        }
    }

    void ApplyResolution(int resolutionIndex, int screenModeIndex)
    {
        if (resolutionIndex >= availableResolutions.Length) return;

        Resolution res = availableResolutions[resolutionIndex];
        FullScreenMode mode = GetFullScreenModeFromDropdown(screenModeIndex);


        Screen.SetResolution(res.width, res.height, mode);
    }
    FullScreenMode GetFullScreenModeFromDropdown(int index)
    {
        switch (index)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen; // or FullScreenWindow if you prefer
            case 1:
                return FullScreenMode.Windowed;
            default:
                return FullScreenMode.FullScreenWindow;
        }
    }
    int GetDropdownIndexFromFullScreenMode(FullScreenMode mode)
    {
        switch (mode)
        {
            case FullScreenMode.ExclusiveFullScreen:
            case FullScreenMode.FullScreenWindow:
                return 0;
            case FullScreenMode.Windowed:
                return 1;
            default:
                return 0;
        }
    }
}
