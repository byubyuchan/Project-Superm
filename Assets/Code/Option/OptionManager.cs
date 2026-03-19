using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [Header("Panel Control")]
    public GameObject optionPanel;
    public Button graphicsTabButton;
    public Button controlTabButton;
    public GameObject graphicsPage;
    public GameObject controlPage;
    public Button closeButton;

    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown refreshRateDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown qualityDropdown;
    public Button applyGraphicsButton;

    private List<Vector2Int> uniqueResolutions = new List<Vector2Int>();
    private List<Resolution> currentRefreshRates = new List<Resolution>();

    [Header("Control Settings")]
    public Slider sensitivitySlider;
    public TMP_InputField sensitivityInput;

    [Header("Unsaved Warning UI")]
    public GameObject unsavedWarningPanel;
    public Button popupApplyButton;
    public Button popupDiscardButton;
    public Button popupCancelButton;

    private bool hasUnsavedChanges = false;

    private int savedResIndex;
    private int savedHzIndex;
    private int savedModeIndex;
    private int savedQualityIndex;

    // Action to execute after handling unsaved changes (apply, discard, or cancel)
    private System.Action pendingAction = null;

    void Start()
    {
        graphicsTabButton.onClick.AddListener(AttemptShowGraphicsPage);
        controlTabButton.onClick.AddListener(AttemptShowControlPage);
        closeButton.onClick.AddListener(AttemptCloseOptionPanel);

        InitGraphicsSettings();
        applyGraphicsButton.onClick.AddListener(ApplyGraphicsSettings);
        resolutionDropdown.onValueChanged.AddListener(UpdateRefreshRateDropdown);

        // Mark settings as unsaved when any dropdown value changes
        resolutionDropdown.onValueChanged.AddListener(delegate { hasUnsavedChanges = true; });
        refreshRateDropdown.onValueChanged.AddListener(delegate { hasUnsavedChanges = true; });
        screenModeDropdown.onValueChanged.AddListener(delegate { hasUnsavedChanges = true; });
        qualityDropdown.onValueChanged.AddListener(delegate { hasUnsavedChanges = true; });

        unsavedWarningPanel.SetActive(false);

        popupApplyButton.onClick.AddListener(OnPopupApply);
        popupDiscardButton.onClick.AddListener(OnPopupDiscard);
        popupCancelButton.onClick.AddListener(OnPopupCancel);

        InitControlSettings();
        sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        sensitivityInput.onValueChanged.AddListener(OnSensitivityInputChanged);

        optionPanel.SetActive(false);
    }

    public void OpenOptionPanel()
    {
        optionPanel.SetActive(true);
        ShowGraphicsPage();
    }

    // ==========
    // Tab Change
    // ==========

    public void ShowGraphicsPage()
    {
        graphicsPage.SetActive(true);
        controlPage.SetActive(false);
    }

    public void ShowControlPage()
    {
        graphicsPage.SetActive(false);
        controlPage.SetActive(true);
    }

    public void CloseOptionPanel()
    {
        optionPanel.SetActive(false);
    }

    // =================
    // Graphics Settings
    // =================
    private void InitGraphicsSettings()
    {
        Resolution[] allResolutions = Screen.resolutions;
        uniqueResolutions.Clear();
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            float ratio = (float)allResolutions[i].width / allResolutions[i].height;
            if (Mathf.Abs(ratio - (16f / 9f)) < 0.05f && allResolutions[i].width >= 1280)
            {
                Vector2Int size = new Vector2Int(allResolutions[i].width, allResolutions[i].height);
                if (!uniqueResolutions.Contains(size))
                {
                    uniqueResolutions.Add(size);
                }
            }
        }

        List<string> options = new List<string>();
        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            options.Add(uniqueResolutions[i].x + " x " + uniqueResolutions[i].y);

            if (uniqueResolutions[i].x == Screen.width && uniqueResolutions[i].y == Screen.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        UpdateRefreshRateDropdown(currentResIndex);

        qualityDropdown.value = QualitySettings.GetQualityLevel();

        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) screenModeDropdown.value = 0;
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) screenModeDropdown.value = 1;
        else screenModeDropdown.value = 2;

        SaveCurrentStateAsApplied();
    }

    private void UpdateRefreshRateDropdown(int resIndex)
    {
        refreshRateDropdown.ClearOptions();
        currentRefreshRates.Clear();

        Vector2Int targetSize = uniqueResolutions[resIndex];
        Resolution[] allResolutions = Screen.resolutions;

        List<string> hzOptions = new List<string>();
        int currentHzIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (allResolutions[i].width == targetSize.x && allResolutions[i].height == targetSize.y)
            {
                string hzString = Mathf.RoundToInt((float)allResolutions[i].refreshRateRatio.value).ToString() + " Hz";
                
                if(!hzOptions.Contains(hzString))
                {
                    currentRefreshRates.Add(allResolutions[i]);
                    hzOptions.Add(hzString);

                    int currentScreenHz = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
                    int optionHz = Mathf.RoundToInt((float)allResolutions[i].refreshRateRatio.value);

                    if (currentScreenHz == optionHz)
                    {
                        currentHzIndex = currentRefreshRates.Count - 1;
                    }
                }
            }
        }

        if (currentHzIndex == 0 && currentRefreshRates.Count > 0)
        {
            currentHzIndex = currentRefreshRates.Count - 1; // Default to highest refresh rate if current isn't found
        }

        refreshRateDropdown.AddOptions(hzOptions);
        refreshRateDropdown.value = currentHzIndex;
        refreshRateDropdown.RefreshShownValue();
    }

    private void ApplyGraphicsSettings()
    {
        Resolution res = currentRefreshRates[refreshRateDropdown.value];

        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (screenModeDropdown.value == 1) mode = FullScreenMode.FullScreenWindow;
        else if (screenModeDropdown.value == 2) mode = FullScreenMode.Windowed;

        Screen.SetResolution(res.width, res.height, mode);

        QualitySettings.SetQualityLevel(qualityDropdown.value);

        SaveCurrentStateAsApplied();
    }

    // ==========================================================
    // Unsaved Changes Handling and Intercepting Close/Tab Switch
    // ==========================================================

    private void SaveCurrentStateAsApplied()
    {
        savedResIndex = resolutionDropdown.value;
        savedHzIndex = refreshRateDropdown.value;
        savedModeIndex = screenModeDropdown.value;
        savedQualityIndex = qualityDropdown.value;
        hasUnsavedChanges = false; // Reset unsaved changes flag after saving
    }

    private void AttemptShowGraphicsPage() { CheckUnsavedChanges(ShowGraphicsPage); }
    private void AttemptShowControlPage() { CheckUnsavedChanges(ShowControlPage); }
    public void AttemptCloseOptionPanel() { CheckUnsavedChanges(CloseOptionPanel); }

    private void CheckUnsavedChanges(System.Action actionToPerform)
    {
        if (hasUnsavedChanges && graphicsPage.activeSelf)
        {
            pendingAction = actionToPerform;
            unsavedWarningPanel.SetActive(true);
        }
        else
        {
            actionToPerform();
        }
    }

    private void OnPopupApply()
    {
        ApplyGraphicsSettings();
        unsavedWarningPanel.SetActive(false);
        pendingAction?.Invoke();
    }

    private void OnPopupDiscard()
    {
        resolutionDropdown.value = savedResIndex;
        UpdateRefreshRateDropdown(savedResIndex);
        refreshRateDropdown.value = savedHzIndex;
        screenModeDropdown.value = savedModeIndex;
        qualityDropdown.value = savedQualityIndex;

        hasUnsavedChanges = false;
        unsavedWarningPanel.SetActive(false);
        pendingAction?.Invoke();
    }

    private void OnPopupCancel()
    {
        unsavedWarningPanel.SetActive(false);
        pendingAction = null;
    }

    // ================
    // Control Settings
    // ================
    private void InitControlSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);

        sensitivitySlider.value = savedSensitivity;
        sensitivityInput.text = savedSensitivity.ToString("F2");
    }

    private void OnSensitivitySliderChanged(float value)
    {
        sensitivityInput.text = value.ToString("F2");
        SaveSensitivity(value);
    }

    private void OnSensitivityInputChanged(string text)
    {
        if(float.TryParse(text, out float value))
        {
            value = Mathf.Clamp(value, sensitivitySlider.minValue, sensitivitySlider.maxValue);
            sensitivitySlider.value = value;
            SaveSensitivity(value);
        }
    }

    // Save sensitivity to Player's PC permanently
    private void SaveSensitivity(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
