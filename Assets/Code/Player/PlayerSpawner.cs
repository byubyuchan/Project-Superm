using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab; // Inspector에서 PlayerBall 프리팹을 드래그하여 할당
    protected GameObject ball;

    [SerializeField]
    private GameObject canvas;

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 입장했습니다. 플레이어 공을 생성합니다.");
        SpawnPlayer();
        canvas.SetActive(false);
    }

    private void SpawnPlayer()
    {
        // 프리팹이 할당되었는지 확인
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerBall 프리팹이 할당되지 않았습니다. Inspector를 확인하세요.");
            return;
        }

        // PhotonNetwork.Instantiate를 사용하여 네트워크 상에 프리팹을 생성
        // Resources 폴더에 프리팹이 있어야 합니다.
        ball = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.up * 2f, Quaternion.identity);
    }

    // 플레이어가 방을 떠날 때 (옵션)
    public override void OnLeftRoom()
    {
        Debug.Log("방을 떠났습니다.");
        Destroy(ball);
    }

    // 방 생성이 실패했을 때 (옵션)
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    // 방 참가가 실패했을 때 (옵션)
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 참가 실패: {message}");
    }
}