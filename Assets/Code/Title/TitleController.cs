using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pressAnyButtonText;
    public GameObject loginButtonGroup;
    public GameObject loginFailedPopup;

    [Header("Scene Management")]
    public string nextSceneName = "Lobby";

    private bool isWaitingForInput = true;

    private void Start()
    {
        if (pressAnyButtonText != null) pressAnyButtonText.SetActive(true);
        if (loginButtonGroup != null) loginButtonGroup.SetActive(false);
        if (loginFailedPopup != null) loginFailedPopup.SetActive(false);
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            bool isAnyButtonPressed = false;

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                isAnyButtonPressed = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                isAnyButtonPressed = true;
            }

            if (isAnyButtonPressed)
            {
                TransitionToLogin();
            }
        }
    }

    private void TransitionToLogin()
    {
        isWaitingForInput = false;
        if (pressAnyButtonText != null) pressAnyButtonText.SetActive(false);
        if (loginButtonGroup != null) loginButtonGroup.SetActive(true);

        // 파티클 효과나 사운드를 추가하면 좋을 듯
    }

    public void OnLoginButtonClicked()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnTestLoginFailedButtonClicked()
    {
        if (loginFailedPopup != null)
        {
            loginFailedPopup.SetActive(true);
        }
    }

    public void OnClosePopupClicked()
    {
        if (loginFailedPopup != null)
        {
            loginFailedPopup.SetActive(false);
        }
    }
}