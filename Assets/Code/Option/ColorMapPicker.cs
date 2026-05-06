using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorMapPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI References")]
    public RectTransform colorMapRect;
    public RectTransform cursorRect;
    public Image[] previewImages;

    private Texture2D colorTexture;

    private void Awake()
    {
        colorTexture = GetComponent<Image>().sprite.texture;
    }

    private void OnEnable()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#FFFFFF");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor))
        {
            ApplyColorToPreviews(savedColor);
        }
    }

    public void OnPointerDown(PointerEventData eventData) { HandleColorSelection(eventData); }
    public void OnDrag(PointerEventData eventData) { HandleColorSelection(eventData); }

    private void HandleColorSelection(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(colorMapRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float width = colorMapRect.rect.width;
            float height = colorMapRect.rect.height;

            localPoint.x = Mathf.Clamp(localPoint.x, -width / 2, width / 2);
            localPoint.y = Mathf.Clamp(localPoint.y, -height / 2, height / 2);

            cursorRect.anchoredPosition = localPoint;

            float u = (localPoint.x + width / 2) / width;
            float v = (localPoint.y + height / 2) / height;

            Color sampledColor = colorTexture.GetPixelBilinear(u, v);

            SaveAndApplyColor(sampledColor);
        }
    }

    private void SaveAndApplyColor(Color color)
    {
        string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString("CrosshairColorHex", hexColor);
        PlayerPrefs.Save();

        ApplyColorToPreviews(color);

        BaseOptionManager.OnCrosshairColorChanged?.Invoke(color);
    }

    private void ApplyColorToPreviews(Color color)
    {
        if (previewImages != null)
        {
            foreach (Image img in previewImages)
            {
                if (img != null)
                {
                    img.color = color;
                }
            }
        }
    }
}
