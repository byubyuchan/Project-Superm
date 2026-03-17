using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown qualityDropdown;
    public Button applyGraphicsButton;

    [Header("Control Settings")]
    public Slider sensitivitySlider;
    public TMP_InputField sensitivityInput;

    private List<Resolution> filteredResolutions = new List<Resolution>();

    void Start()
    {
        graphicsTabButton.onClick.AddListener(ShowGraphicsPage);
        controlTabButton.onClick.AddListener(ShowControlPage);
        closeButton.onClick.AddListener(CloseOptionPanel);

        InitGraphicsSettings();
        applyGraphicsButton.onClick.AddListener(ApplyGraphicsSettings);

        InitControlSettings();
        sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        sensitivityInput.onValueChanged.AddListener(OnSensitivityInputChanged);

        optionPanel.SetActive(false);
        ShowGraphicsPage();
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
        filteredResolutions.Clear();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            float ratio = (float)allResolutions[i].width / allResolutions[i].height;

            bool is16by9 = Mathf.Abs(ratio - (16f / 9f)) < 0.05f;

            if (is16by9 && allResolutions[i].width >= 1280) // Filter for 16:9 and minimum width
            {
                filteredResolutions.Add(allResolutions[i]);
            }
        }

        for(int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height + 
                " (" + filteredResolutions[i].refreshRateRatio.value.ToString("F0") + "Hz)";
            options.Add(option);

            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        qualityDropdown.value = QualitySettings.GetQualityLevel();

        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) screenModeDropdown.value = 0;
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) screenModeDropdown.value = 1;
        else screenModeDropdown.value = 2;
    }

    private void ApplyGraphicsSettings()
    {
        Resolution res = filteredResolutions[resolutionDropdown.value];

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
