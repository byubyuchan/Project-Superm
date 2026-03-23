using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingPlatform : MonoBehaviourPun
{
    protected List<CharacterController> playersOnPlatform = new List<CharacterController>();

    protected virtual void FixedUpdate()
    {
        // 마스터 클라이언트만 물리적 위치/회전 계산 (나머지는 PhotonTransformView가 동기화)
        if (PhotonNetwork.IsMasterClient)
        {
            HandlePlatformMovement();
        }

        UpdatePlayerPositions();
    }

    // 자식 클래스에서 각각 구현할 움직임 로직
    protected abstract void HandlePlatformMovement();
    protected abstract void UpdatePlayerPositions();

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null && !playersOnPlatform.Contains(cc)) playersOnPlatform.Add(cc);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null) playersOnPlatform.Remove(cc);
        }
    }
}