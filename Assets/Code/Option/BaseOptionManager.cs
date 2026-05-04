using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseOptionManager : MonoBehaviour
{
    [Header("Panel Control")]
    public GameObject optionPanel;
    public Button graphicsTabButton;
    public Button controlTabButton;
    public GameObject graphicsPage;
    public GameObject controlPage;
    public Button closeButton;

    [Header("Common Graphics Settings")]
    public TMP_Dropdown qualityDropdown;
    public Button applyGraphicsButton;

    [Header("Unsaved Warning UI")]
    public GameObject unsavedWarningPanel;
    public Button popupApplyButton;
    public Button popupDiscardButton;
    public Button popupCancelButton;

    public static System.Action<Color> OnCrosshairColorChanged;

    protected bool hasUnsavedChanges = false;
    protected int savedQualityIndex;
    protected System.Action pendingAction = null;

    protected virtual void Start()
    {
        graphicsTabButton.onClick.AddListener(AttemptShowGraphicsPage);
        controlTabButton.onClick.AddListener(AttemptShowControlPage);
        closeButton.onClick.AddListener(AttemptCloseOptionPanel);

        applyGraphicsButton.onClick.AddListener(ApplyGraphicsSettings);
        qualityDropdown.onValueChanged.AddListener(delegate { hasUnsavedChanges = true; });

        unsavedWarningPanel.SetActive(false);

        popupApplyButton.onClick.AddListener(OnPopupApply);
        popupDiscardButton.onClick.AddListener(OnPopupDiscard);
        popupCancelButton.onClick.AddListener(OnPopupCancel);

        optionPanel.SetActive(false);

        savedQualityIndex = QualitySettings.GetQualityLevel();
        qualityDropdown.value = savedQualityIndex;

        InitPlatformGraphics();
        InitPlatformControls();

        hasUnsavedChanges = false;
    }

    public void OpenOptionPanel()
    {
        UIVisibility visibility = GetComponent<UIVisibility>();
        if (visibility != null)
        {
            bool isMobile = SystemInfo.deviceType == DeviceType.Handheld;
            if (isMobile && !visibility.showOnMobile) return;
            if (!isMobile && !visibility.showOnPC) return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel(optionPanel, AttemptCloseOptionPanel);
        else
            optionPanel.SetActive(true);

        ShowGraphicsPage();
    }

    public void CloseOptionPanel()
    {
        optionPanel.SetActive(false);
    }

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

    protected void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        savedQualityIndex = qualityDropdown.value;

        ApplyPlatformGraphics();

        hasUnsavedChanges = false;
    }

    private void AttemptShowGraphicsPage() { CheckUnsavedChanges(ShowGraphicsPage); }
    private void AttemptShowControlPage() { CheckUnsavedChanges(ShowControlPage); }
    public void AttemptCloseOptionPanel() { CheckUnsavedChanges(CloseOptionPanel); }

    private void CheckUnsavedChanges(System.Action actionToPerform)
    {
        if (hasUnsavedChanges && graphicsPage.activeSelf)
        {
            pendingAction = actionToPerform;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPanel(unsavedWarningPanel, OnPopupCancel);
            else
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
        qualityDropdown.value = savedQualityIndex;

        RevertPlatformGraphics();

        hasUnsavedChanges = false;
        unsavedWarningPanel.SetActive(false);
        pendingAction?.Invoke();
    }

    private void OnPopupCancel()
    {
        unsavedWarningPanel.SetActive(false);
        pendingAction = null;
    }

    protected abstract void InitPlatformGraphics();
    protected abstract void ApplyPlatformGraphics();
    protected abstract void RevertPlatformGraphics();
    protected abstract void InitPlatformControls();
}