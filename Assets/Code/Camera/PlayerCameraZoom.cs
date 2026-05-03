using Photon.Pun;
using UnityEngine;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

// 플레이어가 공격 준비 (Aim) 상태일 때 카메라를 줌인하는 스크립트
public class PlayerCameraZoom : MonoBehaviourPun
{
    [Header("Dependencies")]
    public MoveByKeys playerMovement;

    [Header("Camera Zoom Settings")]
    public Transform cameraTransform;
    public float zoomXOffset = 3.6f;
    public float zoomYOffset = 3.6f;
    public float zoomFOV = 40f;
    public float zoomSpeed = 5f;

    public GameObject crosshairImage;

    private float defaultXOffset;
    private float defaultYOffset;
    private float defaultFOV;
    private Camera camComponent;

    private bool isInit= false;

    void Awake()
    {
        // 최초 생성 시의 값을 보존합니다.
        if (!isInit)
        {
            if (cameraTransform == null) cameraTransform = Camera.main.transform;
            camComponent = cameraTransform.GetComponent<Camera>();

            defaultXOffset = cameraTransform.localPosition.x;
            defaultYOffset = cameraTransform.localPosition.y;
            defaultFOV = camComponent.fieldOfView;
            isInit = true;
        }
    }

    // 플레이어가 풀링을 통해 재사용되기 때문에 Awake 이후 OnEnable에서 초기화 작업을 수행
    void OnEnable()
    {
        if (!photonView.IsMine) return;

        if (playerMovement != null) playerMovement.isLoadingAttack = false;

        crosshairImage = GameObject.FindWithTag("Crosshair");
        if (crosshairImage != null) crosshairImage.SetActive(false);
    }

    // 성능은 좋지 않지만 Update를 통해 지속적으로 플레이어가 공격 준비상태인지 아닌지 확인하며 FOV의 변화를 적용
    void Update()
    {
        if (!photonView.IsMine || playerMovement == null) return;
        HandleZoom(playerMovement.isLoadingAttack);
    }

    // 줌인과 줌아웃이 급격하게 일어나지 않게 하기 위해 Lerp를 사용하여 카메라의 위치와 FOV를 부드럽게 변화시키는 함수
    void HandleZoom(bool isAiming)
    {
        if (crosshairImage == null)
        {
            crosshairImage = GameObject.FindWithTag("Crosshair");

            if (crosshairImage == null) return;
        }

        float targetX = isAiming ? zoomXOffset : defaultXOffset;
        float targetY = isAiming ? zoomYOffset : defaultYOffset;
        float targetFOV = isAiming ? zoomFOV : defaultFOV;

        if (isAiming) crosshairImage.gameObject.SetActive(true);
        else crosshairImage.gameObject.SetActive(false);

        Vector3 localPos = cameraTransform.localPosition;
        localPos.x = Mathf.Lerp(localPos.x, targetX, Time.deltaTime * zoomSpeed);
        localPos.y = Mathf.Lerp(localPos.y, targetY, Time.deltaTime * zoomSpeed);
        cameraTransform.localPosition = localPos;

        camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}