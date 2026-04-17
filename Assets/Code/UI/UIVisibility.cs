using UnityEngine;
using UnityEngine.InputSystem;

public class UIVisibility : MonoBehaviour
{
    [Header("Base Platform Setting")]
    public bool showOnPC = true;
    public bool showOnMobile = true;

    [Header("Gamepad Setting")]
    [Tooltip("Hide this UI when a gamepad is connected")]
    public bool hideIfGamepadConnected = true;

    private void Update()
    {
        // Check platform If PC or Mobile
        bool isMobile = Application.isMobilePlatform;

        // Check if a gamepad is connected
        bool isGamepadConnected = Gamepad.current != null;

        // Determine if the UI should be shown based on platform and gamepad status
        bool shouldShow = true;

        if (isGamepadConnected && hideIfGamepadConnected)
        {
            // Hide the UI if a gamepad is connected and the setting is enabled
            shouldShow = false;
        }
        else
        {
            // Follow the original platform settings when no gamepad is connected
            if (isMobile && !showOnMobile) shouldShow = false;
            if (!isMobile && !showOnPC) shouldShow = false;
        }

        // Set the active state of the GameObject based on the determined visibility
        if (gameObject.activeSelf != shouldShow)
        {
            gameObject.SetActive(shouldShow);
        }
    }
}