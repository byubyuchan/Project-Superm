using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RunGameManager : BaseGameManager
{
    [Header("In-Game Player List UI")]
    public Transform playerListPanel;
    public GameObject playerSlotPrefab;

    private Dictionary<int, RunPlayerSlot> playerSlots = new Dictionary<int, RunPlayerSlot>();

    private void Start()
    {
        InitializeUI();
    }
    void InitializeUI()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject obj = Instantiate(playerSlotPrefab, playerListPanel);
            RunPlayerSlot slot = obj.GetComponent<RunPlayerSlot>();
            slot.Setup(p);
            playerSlots.Add(p.ActorNumber, slot);
        }
    }

    private void SortPlayerUI()
    {
        // 1. 점수 기준으로 플레이어 리스트 정렬 (CustomProperties에 "Score"가 있다고 가정)
        var sortedPlayers = PhotonNetwork.PlayerList
            .OrderByDescending(p => p.CustomProperties.ContainsKey("Score") ? (int)p.CustomProperties["Score"] : 0)
            .ToList();

        // 2. 정렬된 순서대로 UI의 Hierarchy 순서 변경 (SetSiblingIndex)
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            int actorNr = sortedPlayers[i].ActorNumber;
            if (playerSlots.ContainsKey(actorNr))
            {
                // UI 목록에서 순서를 맨 위(0)부터 차례대로 배치
                playerSlots[actorNr].transform.SetSiblingIndex(i);

                // 점수 텍스트도 최신화
                int currentScore = sortedPlayers[i].CustomProperties.ContainsKey("Score") ? (int)sortedPlayers[i].CustomProperties["Score"] : 0;
                playerSlots[actorNr].UpdateScore(currentScore);

                bool isFirstPlace = (i == 0 && currentScore > 0);
                playerSlots[actorNr].SetRankIcon(isFirstPlace);
            }
        }
    }

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
            int currentLap = 0;

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
            {
                currentLap = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            }

            currentLap++; // 바퀴 수 1 증가!

            // 3. 포톤 서버에 내 바퀴 수 업데이트 (이걸 해야 남들 화면에서도 내 점수가 올라감)
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props.Add("Score", currentLap);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            Debug.Log($"{currentLap} 바퀴 통과!");

            // 4. 만약 5바퀴를 다 돌았다면 그제서야 진짜 게임 종료
            if (currentLap >= 3)
            {
                FinishGame();
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            // 바뀐 바퀴 수를 UI 슬롯에 업데이트
            if (playerSlots.ContainsKey(targetPlayer.ActorNumber))
            {
                int updatedLap = (int)changedProps["Score"];
                playerSlots[targetPlayer.ActorNumber].UpdateScore(updatedLap);
            }

            SortPlayerUI();
        }
    }

    private void FinishGame()
    {
        currentState = GameState.Finish;
        Debug.Log("결승선 통과! 게임 종료");

        photonView.RPC("RPC_FinishGameUI", RpcTarget.All);
    }
}