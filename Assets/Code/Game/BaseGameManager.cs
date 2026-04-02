using Photon.Pun;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseGameManager : MonoBehaviourPunCallbacks
{
    public enum GameState { Wait, Playing, Finish }
    protected GameState currentState = GameState.Wait;

    [Header("GameEnd")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI countdownText;

    protected void FinishGame()
    {
        // 변수 선언
        var winner = PhotonNetwork.PlayerList
        .OrderByDescending(p => p.CustomProperties.ContainsKey("Score") ? (int)p.CustomProperties["Score"] : 0)
        .FirstOrDefault();

        string winnerName = (winner != null) ? winner.NickName : "Null";

        photonView.RPC("RPC_FinishGameUI", RpcTarget.All, winnerName);
    }

    // 1. 종료 시 호출될 RPC (모든 인원 화면에 결과 UI를 띄우거나 알림)
    [PunRPC]
    protected virtual void RPC_FinishGameUI(string winnerNickName)
    {
        currentState = GameState.Finish;
        winnerText.text = $"{winnerNickName}님이 승리하셨습니다!";
        winnerText.gameObject.SetActive(true);

        // 방장만 로비 이동 타이머를 시작합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_EndCountdown", RpcTarget.All);
        }
    }

    //private IEnumerator WaitAndReturnToWarmup(float waitTime)
    //{
    //    yield return new WaitForSeconds(waitTime);

    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonNetwork.CurrentRoom.IsOpen = true; // 방을 다시 열어서 다른 플레이어가 들어올 수 있게 합니다.
    //    }

    //    // 포톤의 LoadLevel을 사용하면 모든 인원이 함께 씬을 이동합니다.
    //    // PhotonNetwork.AutomaticallySyncScene = true; 설정이 되어있어야 합니다.
    //    PhotonNetwork.LoadLevel("WarmupScene"); // 실제 로비 씬 이름으로 변경
    //}

    protected abstract void CheckFinish();

    [PunRPC]
    protected void RPC_EndCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    protected System.Collections.IEnumerator CountdownCoroutine()
    {
        countdownText.gameObject.SetActive(true);

        int count = 5;
        while (count > 0)
        {
            countdownText.text = count.ToString() + "초뒤 게임이 종료됩니다!";
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "대기실로 이동합니다.";
        yield return new WaitForSeconds(1f);

        if (PhotonNetwork.IsMasterClient)
        {
            // 방을 다시 열어서 다른 플레이어가 들어올 수 있게 합니다
            PhotonNetwork.CurrentRoom.IsOpen = true;
            // 방을 공개 상태로 변경
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.LoadLevel("WarmupScene");
        }
    }

}