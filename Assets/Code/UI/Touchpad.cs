using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;

public class Touchpad : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Camera Rotation (Drag)")]
    [InputControl(layout = "Vector2")]
    [SerializeField] private string m_LookControlPath;
    protected override string controlPathInternal { get => m_LookControlPath; set => m_LookControlPath = value; }
    public float lookSensitivity = 0.5f;

    [Header("Attack (Tap)")]
    public OnScreenButton virtualAttackButton;

    [Header("Judgment Criteria Setting")]
    public float tapTimeLimit = 0.2f; // 탭으로 간주되는 최대 시간
    public float tapDistanceLimit = 20f; // 탭으로 간주되는 최대 이동 거리

    private Vector2 pointerDownPosition;
    private float pointerDownTime;
    private bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 시작 시 위치와 시간을 기록
        pointerDownPosition = eventData.position;
        pointerDownTime = Time.unscaledTime;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(eventData.position, pointerDownPosition) > tapDistanceLimit)
        {
            isDragging = true; // 이동 거리가 탭 기준을 초과하면 드래그로 간주
        }

        if (isDragging)
        {
            SendValueToControl(eventData.delta * lookSensitivity);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SendValueToControl(Vector2.zero); // 드래그가 끝나면 카메라 회전 정지

        // 드래그가 아니고, 탭으로 간주되는 시간과 이동 거리 내에 있다면 공격 트리거
        if (!isDragging && (Time.unscaledTime - pointerDownTime) <= tapTimeLimit)
        {
            if (virtualAttackButton != null)
            {
                // 인풋 시스템 정식 루트로 '버튼 눌림(1)' 신호 전송
                virtualAttackButton.OnPointerDown(eventData);

                // 0.05초 뒤에 떼는 동작 실행
                StartCoroutine(ReleaseAttackButton(eventData));
            }
        }
    }

    private IEnumerator ReleaseAttackButton(PointerEventData eventData)
    {
        yield return new WaitForSeconds(0.05f);
        if (virtualAttackButton != null)
        {
            // 인풋 시스템 정식 루트로 '버튼 뗌(0)' 신호 전송
            virtualAttackButton.OnPointerUp(eventData);
        }
    }
}
