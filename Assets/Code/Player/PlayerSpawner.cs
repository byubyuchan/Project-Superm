using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab; // Inspector에서 PlayerBall 프리팹을 드래그하여 할당
    protected GameObject ball;

    [SerializeField]
    private GameObject canvas;

    void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        float timeout = 5f;
        while (!PhotonNetwork.InRoom && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
            if (canvas != null) canvas.SetActive(false);
        }
        else
        {
            Debug.LogError("방 입장 대기 시간 초과! 네트워크 연결을 확인하세요.");
        }
    }

    public void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        // 다시 한 번 방 상태를 체크하고 생성
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, transform.position, Quaternion.identity);
        }
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