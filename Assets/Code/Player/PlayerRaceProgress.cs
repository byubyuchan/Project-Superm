using UnityEngine;
using Photon.Pun;

public class PlayerRaceProgress : MonoBehaviourPun
{
    private int currentProgress = 0; // 현재 진행도 ex) 0: 시작, 1: 체크포인트1, 2: 체크포인트2, 3: 결승선
    private int nextCheckpointIndex = 0; // 다음 체크포인트 인덱스

    // 초기 위치와 회전 저장
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // 마지막 체크포인트 위치와 회전 저장
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    private void Start()
    {
        if(photonView.IsMine)
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.CompareTag("Checkpoint"))
        {
            if (RunGameManager.Instance == null || RunGameManager.Instance.checkpoints.Count == 0) return;

            Transform expectedCheckpoint = RunGameManager.Instance.checkpoints[nextCheckpointIndex];

            if (other.transform == expectedCheckpoint)
            {
                lastCheckpointPosition = expectedCheckpoint.position;
                lastCheckpointRotation = expectedCheckpoint.rotation;

                currentProgress++;
                nextCheckpointIndex++;

                int currentLap = 0;
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
                {
                    currentLap = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
                }

                if (nextCheckpointIndex >= RunGameManager.Instance.checkpoints.Count)
                {
                    nextCheckpointIndex = 0;
                    currentLap++;

                    TeleportToStart();
                }

                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("Score", currentLap);
                props.Add("Progress", currentProgress);
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                //if (currentLap >= 3)
                //{
                //    if (RunGameManager.Instance != null)
                //    {
                //        RunGameManager.Instance.OnPlayerFinished();
                //    }
                //}
            }
            else
            {
            }
        }
    }

    public void Respawn()
    {
        if (!photonView.IsMine) return; // 내 캐릭터에 대해서만 처리

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // 캐릭터 컨트롤러 비활성화

        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.linearVelocity = Vector3.zero; // 속도 초기화
        //    rb.angularVelocity = Vector3.zero; // 각속도 초기화
        //}

        transform.position = lastCheckpointPosition; // 마지막 체크포인트 위치로 이동
        transform.rotation = lastCheckpointRotation; // 마지막 체크포인트 회전으로 설정

        if (cc != null) cc.enabled = true; // 캐릭터 컨트롤러 재활성화
        cc.GetComponent<PhotonView>().RPC("RPC_SizeReset", RpcTarget.All);
    }

    private void TeleportToStart()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // 캐릭터 컨트롤러 비활성화

        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.linearVelocity = Vector3.zero; // 속도 초기화
        //    rb.angularVelocity = Vector3.zero; // 각속도 초기화
        //}

        transform.position = initialPosition; // 초기 위치로 이동
        transform.rotation = initialRotation; // 초기 회전으로 설정

        lastCheckpointPosition = initialPosition; // 마지막 체크포인트 위치도 초기 위치로 리셋
        lastCheckpointRotation = initialRotation; // 마지막 체크포인트 회전도 초기 회전으로 리셋

        if (cc != null) cc.enabled = true; // 캐릭터 컨트롤러 재활성화
        cc.GetComponent<PhotonView>().RPC("RPC_SizeReset", RpcTarget.All);
    }
}
