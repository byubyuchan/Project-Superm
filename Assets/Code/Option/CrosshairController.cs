using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image smallDotImage;
    public Image largeDotImage;

    private void Start()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor)) ApplyColor(savedColor);

        int savedShape = PlayerPrefs.GetInt("CrosshairShape", 0);
        ApplyShape(savedShape);
    }

    private void OnEnable()
    {
        BaseOptionManager.OnCrosshairColorChanged += ApplyColor;
        BaseOptionManager.OnCrosshairShapeChanged += ApplyShape;
    }

    private void OnDisable()
    {
        BaseOptionManager.OnCrosshairColorChanged -= ApplyColor;
        BaseOptionManager.OnCrosshairShapeChanged -= ApplyShape;
    }

    private void ApplyColor(Color newColor)
    {
        if (smallDotImage != null) smallDotImage.color = newColor;
        if (largeDotImage != null) largeDotImage.color = newColor;
    }

    private void ApplyShape(int shapeIndex)
    {
        if (smallDotImage != null) smallDotImage.gameObject.SetActive(shapeIndex == 0);
        if (largeDotImage != null) largeDotImage.gameObject.SetActive(shapeIndex == 1);
    }
}
