using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image circleImage;
    public Image dotImage;

    private void Start()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor)) ApplyColor(savedColor);

        int savedShape = PlayerPrefs.GetInt("CrosshairShape", 1);
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
        if (circleImage != null) circleImage.color = newColor;
        if (dotImage != null) dotImage.color = newColor;
    }

    private void ApplyShape(int shapeIndex)
    {
        if (circleImage != null) circleImage.gameObject.SetActive(shapeIndex == 0);
        if (dotImage != null) dotImage.gameObject.SetActive(shapeIndex == 1);
    }
}
