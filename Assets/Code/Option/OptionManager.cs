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

    void Start()
    {
        graphicsTabButton.onClick.AddListener(ShowGraphicsPage);
        controlTabButton.onClick.AddListener(ShowControlPage);
        closeButton.onClick.AddListener(CloseOptionPanel);

        InitGraphicsSettings();
        applyGraphicsButton.onClick.AddListener(ApplyGraphicsSettings);
        resolutionDropdown.onValueChanged.AddListener(UpdateRefreshRateDropdown);

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
                currentRefreshRates.Add(allResolutions[i]);
                hzOptions.Add(allResolutions[i].refreshRateRatio.value.ToString("F0") + " Hz");

                if (Mathf.Approximately((float)allResolutions[i].refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
                {
                    currentHzIndex = currentRefreshRates.Count - 1;
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
        Resolution res = currentRefreshRates[resolutionDropdown.value];

        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (screenModeDropdown.value == 1) mode = FullScreenMode.FullScreenWindow;
        else if (screenModeDropdown.value == 2) mode = FullScreenMode.Windowed;

        Screen.SetResolution(res.width, res.height, mode);

        QualitySettings.SetQualityLevel(qualityDropdown.value);

        Debug.Log("Graphics settings applied");
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
