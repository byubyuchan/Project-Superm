using Photon.Pun;
using UnityEngine;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

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

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        if (playerMovement != null) playerMovement.isLoadingAttack = false;

        crosshairImage = GameObject.FindWithTag("Crosshair");
        if (crosshairImage != null) crosshairImage.SetActive(false);
    }

    void Update()
    {
        if (!photonView.IsMine || playerMovement == null) return;
        HandleZoom(playerMovement.isLoadingAttack);
    }

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