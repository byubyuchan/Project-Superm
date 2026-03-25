using UnityEngine;
using Photon.Pun;

public class PlayerRaceProgress : MonoBehaviourPun
{
    private int currentProgress = 0; // 현재 진행도 ex) 0: 시작, 1: 체크포인트1, 2: 체크포인트2, 3: 결승선
    private int nextCheckpointIndex = 0; // 다음 체크포인트 인덱스

    public int totalCheckpointsPerLap = 10; // 한 바퀴당 체크포인트 수 (결승선 포함)

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    private void Start()
    {
        // 초기 위치와 회전 저장
        if(photonView.IsMine)
        {
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return; // 내 캐릭터에 대해서만 처리

        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint cp = other.GetComponent<Checkpoint>();

            if (cp != null && cp.index == nextCheckpointIndex)
            {
                lastCheckpointPosition = cp.transform.position;
                lastCheckpointRotation = cp.transform.rotation;

                currentProgress++;
                nextCheckpointIndex++;

                int currentLap = 0;
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
                {
                    currentLap = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
                }

                if (nextCheckpointIndex >= totalCheckpointsPerLap)
                {
                    nextCheckpointIndex = 0; // 다음 랩의 첫 체크포인트로 리셋
                    currentLap++; // 랩 증가
                }

                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props.Add("Score", currentLap);
                props.Add("Progress", currentProgress);
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                if (currentLap >= 3)
                {
                    if (RunGameManager.Instance != null)
                    {
                        RunGameManager.Instance.OnPlayerFinished();
                    }
                }
            }
        }
    }

    public void Respawn()
    {
        if (!photonView.IsMine) return; // 내 캐릭터에 대해서만 처리

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // 캐릭터 컨트롤러 비활성화

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // 속도 초기화
            rb.angularVelocity = Vector3.zero; // 각속도 초기화
        }

        transform.position = lastCheckpointPosition; // 마지막 체크포인트 위치로 이동
        transform.rotation = lastCheckpointRotation; // 마지막 체크포인트 회전으로 설정

        if (cc != null) cc.enabled = true; // 캐릭터 컨트롤러 재활성화
    }
}
