using Photon.Pun;
using UnityEngine;
using Photon.Pun.UtilityScripts;

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

    private float defaultXOffset;
    private float defaultYOffset;
    private float defaultFOV;
    private Camera camComponent;

    void Start()
    {
        // 본인 소유가 아니면 작동 안함
        if (!photonView.IsMine) return;

        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        camComponent = cameraTransform.GetComponent<Camera>();

        // 초기값 저장
        defaultXOffset = cameraTransform.localPosition.x;
        defaultYOffset = cameraTransform.localPosition.y;
        defaultFOV = camComponent.fieldOfView;
    }

    void Update()
    {
        if (!photonView.IsMine || playerMovement == null) return;
        HandleZoom(playerMovement.isLoadingAttack);
    }

    void HandleZoom(bool isAiming)
    {
        float targetX = isAiming ? zoomXOffset : defaultXOffset;
        float targetY = isAiming ? zoomYOffset : defaultYOffset;
        float targetFOV = isAiming ? zoomFOV : defaultFOV;

        Vector3 localPos = cameraTransform.localPosition;
        localPos.x = Mathf.Lerp(localPos.x, targetX, Time.deltaTime * zoomSpeed);
        localPos.y = Mathf.Lerp(localPos.y, targetY, Time.deltaTime * zoomSpeed);
        cameraTransform.localPosition = localPos;

        camComponent.fieldOfView = Mathf.Lerp(camComponent.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}