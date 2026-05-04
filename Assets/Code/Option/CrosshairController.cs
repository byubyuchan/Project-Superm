using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image dotImage;
    public Image circleImage;

    private void Start()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColor", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor))
        {
            ApplyColor(savedColor);
        }
    }

    private void OnEnable()
    {
        BaseOptionManager.OnCrosshairColorChanged += ApplyColor;
    }

    private void OnDisable()
    {
        BaseOptionManager.OnCrosshairColorChanged -= ApplyColor;
    }

    private void ApplyColor(Color newColor)
    {
        if (dotImage != null) dotImage.color = newColor;
        if (circleImage != null) circleImage.color = newColor;
    }
}
