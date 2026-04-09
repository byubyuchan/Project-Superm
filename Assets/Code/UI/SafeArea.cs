using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        Refresh();
    }

    // 스마트폰을 회전할 때마다 Safe Area가 변경될 수 있으므로 매 프레임마다 체크
    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        Rect safeArea = Screen.safeArea;

        // Safe Area가 변경되면 UI를 업데이트
        if (safeArea != lastSafeArea)
        {
            lastSafeArea = safeArea;
            ApplySafeArea(safeArea);
        }
    }

    void ApplySafeArea(Rect r)
    {
        // Safe Area의 위치와 크기를 화면 비율로 계산
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // 패널의 앵커를 Safe Area에 맞게 설정
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}
