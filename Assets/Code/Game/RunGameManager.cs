using UnityEngine;
using Photon.Pun;

public class RunGameManager : BaseGameManager
{
    // 부모에서 abstract로 선언했을 경우 override 사용
    protected override void CheckFinish()
    {
        // 트리거 방식을 쓸 때는 Update에서 매번 체크할 필요가 없으므로 
        // 이 함수는 비워두거나, 다른 보조 판정용으로 씁니다.
    }

    // 플레이어가 결승선(Trigger)에 닿았을 때 호출
    public void OnPlayerReachedFinish(GameObject player)
    {
        if (currentState == GameState.Finish) return;

        // 포톤 사용 시: 내 캐릭터가 닿았는지 확인
        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            Debug.Log("루프 맵 결승점 도달!");
            FinishGame();
        }
    }

    private void FinishGame()
    {
        currentState = GameState.Finish;
        Debug.Log("결승선 통과! 게임 종료");

        photonView.RPC("RPC_FinishGameUI", RpcTarget.All);

        // 포톤을 사용 중이라면 MasterClient가 모든 인원에게 종료를 알리는 RPC 호출
        // photonView.RPC("RPC_ShowResult", RpcTarget.All);
    }
}