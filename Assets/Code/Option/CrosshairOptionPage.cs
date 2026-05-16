using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrosshairOptionPage : MonoBehaviour
{
    [Header("Settings UI")]
    public TMP_Dropdown shapeDropdown;
    public Slider sizeSlider;
    public TMP_InputField sizeInput;
    public Toggle cooldownToggle;

    private bool isUpdatingUI = false;

    private void Awake()
    {
        if (shapeDropdown != null)
        {
            shapeDropdown.onValueChanged.AddListener(OnShapeDropdownChanged);
        }

        if (sizeSlider != null)
        {
            sizeSlider.onValueChanged.AddListener(OnSliderSizeChanged);
        }

        if (sizeInput != null)
        {
            sizeInput.onEndEdit.AddListener(OnInputSizeChanged);
        }

        if (cooldownToggle != null)
        {
            cooldownToggle.onValueChanged.AddListener(OnCooldownToggleChanged);
        }
    }

    private void OnEnable()
    {
        isUpdatingUI = true;

        int savedShape = PlayerPrefs.GetInt("CrosshairShape", 0);
        if (shapeDropdown != null) shapeDropdown.value = savedShape;

        float savedSize = PlayerPrefs.GetFloat("CrosshairSize", 1.0f);
        if (sizeSlider != null) sizeSlider.value = savedSize;
        if (sizeInput != null) sizeInput.text = savedSize.ToString("F1");

        int savedCooldown = PlayerPrefs.GetInt("CrosshairCooldownVisible", 1);
        if (cooldownToggle != null) cooldownToggle.isOn = (savedCooldown == 1);

        isUpdatingUI = false;
    }

    private void OnShapeDropdownChanged(int index)
    {
        if (isUpdatingUI) return;

        PlayerPrefs.SetInt("CrosshairShape", index);
        PlayerPrefs.Save();

        BaseOptionManager.OnCrosshairShapeChanged?.Invoke(index);
    }

    private void OnSliderSizeChanged(float value)
    {
        if (isUpdatingUI) return;

        isUpdatingUI = true;

        if (sizeInput != null) sizeInput.text = value.ToString("F1");

        SaveAndBroadcastSize(value);

        isUpdatingUI = false;
    }

    private void OnInputSizeChanged(string text)
    {
        if (isUpdatingUI) return;

        if (float.TryParse(text, out float value))
        {
            isUpdatingUI = true;

            if (sizeSlider != null)
            {
                value = Mathf.Clamp(value, sizeSlider.minValue, sizeSlider.maxValue);
                sizeSlider.value = value;
            }

            if (sizeInput != null) sizeInput.text = value.ToString("F1");

            SaveAndBroadcastSize(value);

            isUpdatingUI = false;
        }
    }

    private void SaveAndBroadcastSize(float size)
    {
        PlayerPrefs.SetFloat("CrosshairSize", size);
        PlayerPrefs.Save();

        BaseOptionManager.OnCrosshairSizeChanged?.Invoke(size);
    }

    private void OnCooldownToggleChanged(bool isOn)
    {
        if (isUpdatingUI) return;

        PlayerPrefs.SetInt("CrosshairCooldownVisible", isOn ? 1 : 0);
        PlayerPrefs.Save();

        BaseOptionManager.OnCooldownVisibilityChanged?.Invoke(isOn);
    }
}