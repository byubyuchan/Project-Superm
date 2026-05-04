using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorMapPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI References")]
    public RectTransform colorMapRect;
    public RectTransform cursorRect;
    public Image previewColorImage;

    private Texture2D colorTexture;

    void Start()
    {
        colorTexture = GetComponent<Image>().sprite.texture;

        string savedHex = PlayerPrefs.GetString("CrosshairColor", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor))
        {
            if (previewColorImage != null) previewColorImage.color = savedColor;
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

            float normalizedX = (localPoint.x + width / 2) / width;
            float normalizedY = (localPoint.y + height / 2) / height;

            Color sampledColor= colorTexture.GetPixelBilinear(normalizedX, normalizedY);

            SaveAndApplyColor(sampledColor);
        }
    }

    private void SaveAndApplyColor(Color color)
    {
        string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString("CrosshairColor", hexColor);
        PlayerPrefs.Save();

        if (previewColorImage != null) previewColorImage.color = color;

        BaseOptionManager.OnCrosshairColorChanged?.Invoke(color);
    }
}
