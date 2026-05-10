using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ColorMapPicker : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("UI References")]
    public RectTransform colorMapRect;
    public RectTransform cursorRect;
    public Image circlePreviewImage;
    public Image dotPreviewImage;

    [Header("RGB Input Fields")]
    public TMP_InputField rInput;
    public TMP_InputField gInput;
    public TMP_InputField bInput;

    [Header("Shape Toggles")]
    public Toggle circleToggle;
    public Toggle dotToggle;

    private Texture2D colorTexture;
    private bool isUpdatingUI = false;

    private void Awake()
    {
        colorTexture = GetComponent<Image>().sprite.texture;

        if (rInput != null) rInput.onValueChanged.AddListener(delegate { OnRGBInputChanged(); });
        if (gInput != null) gInput.onValueChanged.AddListener(delegate { OnRGBInputChanged(); });
        if (bInput != null) bInput.onValueChanged.AddListener(delegate { OnRGBInputChanged(); });

        if (circleToggle != null) circleToggle.onValueChanged.AddListener(delegate { OnShapeChanged(); });
        if (dotToggle != null) dotToggle.onValueChanged.AddListener(delegate { OnShapeChanged(); });
    }

    private void OnEnable()
    {
        string savedHex = PlayerPrefs.GetString("CrosshairColorHex", "#000000");
        if (!ColorUtility.TryParseHtmlString(savedHex, out Color savedColor)) savedColor = Color.black;

        float savedU = PlayerPrefs.GetFloat("CrosshairCursorU", 0.5f);
        float savedV = PlayerPrefs.GetFloat("CrosshairCursorV", 0.0f);

        float width = colorMapRect.rect.width;
        float height = colorMapRect.rect.height;
        cursorRect.localPosition = new Vector2((savedU * width) - (width / 2), (savedV * height) - (height / 2));

        UpdateRGBInputFields(savedColor);
        ApplyColorToPreviews(savedColor);

        int savedShape = PlayerPrefs.GetInt("CrosshairShape", 0);
        isUpdatingUI = true;
        if (savedShape == 0) circleToggle.isOn = true;
        else dotToggle.isOn = true;
        isUpdatingUI = false;

        ApplyShapeToPreviews(savedShape);
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

            cursorRect.localPosition = localPoint;

            float u = (localPoint.x + width / 2) / width;
            float v = (localPoint.y + height / 2) / height;

            Color sampledColor = colorTexture.GetPixelBilinear(u, v);

            if (v >= 0.99f) sampledColor = Color.white;
            if (v <= 0.01f) sampledColor = Color.black;

            SaveAndApplyColor(sampledColor, u, v, true);
        }
    }

    private void OnRGBInputChanged()
    {
        if (isUpdatingUI) return;

        int r = ParseColorValue(rInput.text);
        int g = ParseColorValue(gInput.text);
        int b = ParseColorValue(bInput.text);

        Color newColor = new Color(r / 255f, g / 255f, b / 255f, 1f);

        Color.RGBToHSV(newColor, out float h, out float s, out float v);
        float u = h;
        float mapV = (v < 1f) ? (v / 2f) : (0.5f + (1f - s) / 2f);

        float width = colorMapRect.rect.width;
        float height = colorMapRect.rect.height;
        cursorRect.localPosition = new Vector2((u * width) - (width / 2), (mapV * height) - (height / 2));

        SaveAndApplyColor(newColor, u, mapV, false);
    }

    private int ParseColorValue(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (int.TryParse(text, out int val)) return Mathf.Clamp(val, 0, 255);
        return 0;
    }

    private void OnShapeChanged()
    {
        if (isUpdatingUI) return;

        int shapeIndex = circleToggle.isOn ? 0 : 1;

        PlayerPrefs.SetInt("CrosshairShape", shapeIndex);
        PlayerPrefs.Save();

        ApplyShapeToPreviews(shapeIndex);

        BaseOptionManager.OnCrosshairShapeChanged?.Invoke(shapeIndex);
    }

    private void ApplyShapeToPreviews(int shapeIndex)
    {
        if (circlePreviewImage != null) circlePreviewImage.gameObject.SetActive(shapeIndex == 0);
        if (dotPreviewImage != null) dotPreviewImage.gameObject.SetActive(shapeIndex == 1);
    }

    private void ApplyColorToPreviews(Color color)
    {
        if (circlePreviewImage != null) circlePreviewImage.color = color;
        if (dotPreviewImage != null) dotPreviewImage.color = color;
    }

    private void SaveAndApplyColor(Color color, float u, float v, bool updateInputs)
    {
        string hexColor = "#" + ColorUtility.ToHtmlStringRGBA(color);
        PlayerPrefs.SetString("CrosshairColorHex", hexColor);
        PlayerPrefs.SetFloat("CrosshairCursorU", u);
        PlayerPrefs.SetFloat("CrosshairCursorV", v);
        PlayerPrefs.Save();

        ApplyColorToPreviews(color);
        BaseOptionManager.OnCrosshairColorChanged?.Invoke(color);

        if (updateInputs) UpdateRGBInputFields(color);
    }

    private void UpdateRGBInputFields(Color color)
    {
        isUpdatingUI = true;
        if (rInput != null) rInput.text = Mathf.RoundToInt(color.r * 255).ToString();
        if (gInput != null) gInput.text = Mathf.RoundToInt(color.g * 255).ToString();
        if (bInput != null) bInput.text = Mathf.RoundToInt(color.b * 255).ToString();
        isUpdatingUI = false;
    }
}
