using UnityEngine;
using Photon.Pun;

public class PlayerRaceProgress : MonoBehaviourPun
{
    private int currentProgress = 0; // 현재 진행도 ex) 0: 시작, 1: 체크포인트1, 2: 체크포인트2, 3: 결승선
    private int nextCheckpointIndex = 0; // 다음 체크포인트 인덱스

    public int totalCheckpointsPerLap = 10; // 한 바퀴당 체크포인트 수 (결승선 포함)

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return; // 내 캐릭터에 대해서만 처리

        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint cp = other.GetComponent<Checkpoint>();

            if (cp != null && cp.index == nextCheckpointIndex)
            {
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

                    Debug.Log($"Lap {currentLap} completed!");
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
}
