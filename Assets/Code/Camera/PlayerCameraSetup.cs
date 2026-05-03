using UnityEngine;
using Photon.Pun;

public class PlayerCameraSetup : MonoBehaviourPun
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    // 각자의 플레이어에 맞게 플레이어에 내장된 카메라를 메인 카메라로 변경하는 코드
    void OnEnable()
    {
        if (photonView.IsMine)
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                playerCamera.tag = "MainCamera";
            }

            if (audioListener != null)
            {
                audioListener.enabled = true;
            }

            if (Camera.main != null && Camera.main != playerCamera)
            {
                Camera.main.gameObject.SetActive(false);
            }
        }
        else
        {
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}