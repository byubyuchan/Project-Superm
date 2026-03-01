using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseGameManager : MonoBehaviourPunCallbacks
{
    public enum GameState { Wait, Playing, Finish }
    protected GameState currentState = GameState.Wait;

    // 1. 종료 시 호출될 RPC (모든 인원 화면에 결과 UI를 띄우거나 알림)
    [PunRPC]
    protected virtual void RPC_FinishGameUI()
    {
        currentState = GameState.Finish;
        Debug.Log("모든 클라이언트: 게임 종료 UI 표시");

        // 여기에 결과 UI를 띄우는 코드를 넣으세요.
        // 예: ResultCanvas.SetActive(true);

        // 방장만 로비 이동 타이머를 시작합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitAndReturnToWarmup(5f)); // 5초 뒤 이동
        }
    }

    // 2. 일정 시간 대기 후 로비로 이동하는 코루틴
    private IEnumerator WaitAndReturnToWarmup(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true; // 방을 다시 열어서 다른 플레이어가 들어올 수 있게 합니다.
        }

        // 포톤의 LoadLevel을 사용하면 모든 인원이 함께 씬을 이동합니다.
        // PhotonNetwork.AutomaticallySyncScene = true; 설정이 되어있어야 합니다.
        PhotonNetwork.LoadLevel("WarmupScene"); // 실제 로비 씬 이름으로 변경
    }

    protected abstract void CheckFinish();
}