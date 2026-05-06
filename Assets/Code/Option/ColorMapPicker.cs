using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorMapPicker : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
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
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#000000");
        if (ColorUtility.TryParseHtmlString(savedHex, out Color savedColor))
        {
            ApplyColorToPreviews(savedColor);
        }

        float savedU = PlayerPrefs.GetFloat("CrosshairCursorU", 0.5f);
        float savedV = PlayerPrefs.GetFloat("CrosshairCursorV", 0.0f);

        float width = colorMapRect.rect.width;
        float height = colorMapRect.rect.height;

        Vector2 loadedPoint = new Vector2(
            (savedU * width) - (width / 2),
            (savedV * height) - (height / 2)
        );
        cursorRect.localPosition = loadedPoint;
    }

    public void OnPointerDown(PointerEventData eventData) { HandleColorSelection(eventData); }
    public void OnDrag(PointerEventData eventData) { HandleColorSelection(eventData); }
    // 클릭을 뗐을 때의 이벤트가 없으면 부모인 버튼이 클릭 이벤트를 받아버려서 창이 닫히는 문제가 있어서 빈 구현으로 남겨둠
    public void OnPointerUp(PointerEventData eventData) {  }
    public void OnPointerClick(PointerEventData eventData) {  }

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

            SaveAndApplyColor(sampledColor, u, v);
        }
    }

    private void SaveAndApplyColor(Color color, float u, float v)
    {
        string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString("CrosshairColorHex", hexColor);

        PlayerPrefs.SetFloat("CrosshairCursorU", u);
        PlayerPrefs.SetFloat("CrosshairCursorV", v);

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
