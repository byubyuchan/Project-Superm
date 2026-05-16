using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public Image[] shapeImages;
    public Image cooldownRingImage;

    private bool isCooldownOptionOn = true;

    private void Start()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor)) ApplyColor(savedColor);

        int savedShape = PlayerPrefs.GetInt("CrosshairShape", 0);
        ApplyShape(savedShape);

        isCooldownOptionOn = PlayerPrefs.GetInt("CrosshairCooldownVisible", 1) == 1;
        ApplyCooldownVisibility(isCooldownOptionOn);

        float savedSize = PlayerPrefs.GetFloat("CrosshairSize", 1.0f);
        ApplySize(savedSize);
    }

    private void OnEnable()
    {
        BaseOptionManager.OnCrosshairColorChanged += ApplyColor;
        BaseOptionManager.OnCrosshairShapeChanged += ApplyShape;
        BaseOptionManager.OnCooldownVisibilityChanged += ApplyCooldownVisibility;
        BaseOptionManager.OnCrosshairSizeChanged += ApplySize;
    }

    private void OnDisable()
    {
        BaseOptionManager.OnCrosshairColorChanged -= ApplyColor;
        BaseOptionManager.OnCrosshairShapeChanged -= ApplyShape;
        BaseOptionManager.OnCooldownVisibilityChanged -= ApplyCooldownVisibility;
        BaseOptionManager.OnCrosshairSizeChanged -= ApplySize;
    }

    private void ApplyColor(Color newColor)
    {
        if (shapeImages != null)
        {
            foreach (var img in shapeImages) if (img != null) img.color = newColor;
        }

        if (cooldownRingImage != null)
        {
            Color finalColor = newColor;
            finalColor.a = cooldownRingImage.color.a;
            cooldownRingImage.color = finalColor;
        }
    }

    private void ApplyShape(int shapeIndex)
    {
        if (shapeImages != null)
        {
            for (int i = 0; i < shapeImages.Length; i++)
            {
                if (shapeImages[i] != null) shapeImages[i].gameObject.SetActive(i == shapeIndex);
            }
        }
    }

    private void ApplySize(float size)
    {
        this.transform.localScale = new Vector3(size, size, 1f);
    }

    private void ApplyCooldownVisibility(bool isOn)
    {
        isCooldownOptionOn = isOn;

        if (cooldownRingImage != null)
        {
            cooldownRingImage.gameObject.SetActive(isOn);
        }
    }
}
