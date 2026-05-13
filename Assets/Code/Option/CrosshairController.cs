using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image[] shapeImages;

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
        if (shapeImages != null)
        {
            for (int i = 0; i < shapeImages.Length; i++)
            {
                if (shapeImages[i] != null) shapeImages[i].color = newColor;
            }
        }
    }

    private void ApplyShape(int shapeIndex)
    {
        if (shapeImages != null)
        {
            for (int i = 0; i < shapeImages.Length; i++)
            {
                if (shapeImages[i] != null)
                {
                    shapeImages[i].gameObject.SetActive(i == shapeIndex);
                }
            }
        }
    }
}
