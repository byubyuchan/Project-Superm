using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunGameManager : BaseGameManager
{
    public static RunGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Track Data")]
    public List<Transform> checkpoints = new List<Transform>();
    public int maxLap = 3;

    [Header("In-Game Player List UI")]
    public Transform playerListPanel;
    public GameObject playerSlotPrefab;

    [Header("System Menu UI")]
    public GameObject systemMenuPanel;
    public Button leaveRoomButton;
    public Button cancelButton;

    private int maxPlayers;
    private List<RunPlayerSlot> allSlots = new List<RunPlayerSlot>();
    private Dictionary<int, RunPlayerSlot> activePlayerSlots = new Dictionary<int, RunPlayerSlot>();

    public const string PROP_LAP = "Score";
    public const string PROP_PROG = "Progress";
    public const string PROP_GOAL = "NextCP";

    public const string PROP_INIT_X = "InitX";
    public const string PROP_INIT_Y = "InitY";
    public const string PROP_INIT_Z = "InitZ";
    public const string PROP_INIT_ROT_Y = "InitRotY";

    public const string PROP_LAST_X = "LastX";
    public const string PROP_LAST_Y = "LastY";
    public const string PROP_LAST_Z = "LastZ";
    public const string PROP_LAST_ROT_Y = "LastRotY";

    private void Start()
    {
        maxPlayers = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.MaxPlayers : 8;
        if (maxPlayers == 0) maxPlayers = 8;

        InitializeUI();

        if (systemMenuPanel != null) systemMenuPanel.SetActive(false);
        if (leaveRoomButton != null) leaveRoomButton.onClick.AddListener(LeaveRoom);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseSystemMenu);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.onEmptyEsc = OpenSystemMenu;
        }
    }

    void InitializeUI()
    {
        Player[] rawPlayers = PhotonNetwork.PlayerList;

        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject slotObj = Instantiate(playerSlotPrefab, playerListPanel);
            RunPlayerSlot slot = slotObj.GetComponent<RunPlayerSlot>();
            allSlots.Add(slot);

            if (i < rawPlayers.Length)
            {
                Player p = rawPlayers[i];
                slot.Setup(p);
                activePlayerSlots.Add(p.ActorNumber, slot);
            }
            else
            {
                slot.SetEmpty();
            }
        }
    }

    private void SortPlayerUI()
    {
        // 1. 점수(바퀴 수)를 1순위로 내림차순 정렬
        // 2. 진행도(Distance)를 2순위로 내림차순 정렬 (골인 지점에 가까울수록 수치가 크다고 가정)
        // 만약 '결승선까지 남은 거리'라면 값이 작을수록 앞서있는 것이므로 ThenBy(오름차순)를 사용
        var sortedPlayers = PhotonNetwork.PlayerList
            .OrderByDescending(p => p.CustomProperties.ContainsKey(PROP_LAP) ? (int)p.CustomProperties[PROP_LAP] : 0)
            .ThenByDescending(p => p.CustomProperties.ContainsKey(PROP_PROG) ? (int)p.CustomProperties[PROP_PROG] : 0)
            .ToList();

        int siblingIndex = 0;

        // 2. 정렬된 순서대로 UI의 Hierarchy 순서 변경 (SetSiblingIndex)
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            int actorNr = sortedPlayers[i].ActorNumber;
            if (activePlayerSlots.ContainsKey(actorNr))
            {
                RunPlayerSlot slot = activePlayerSlots[actorNr];
                // SetSiblingIndex로 UI 순서 변경
                slot.transform.SetSiblingIndex(siblingIndex);
                slot.UpdateScore(sortedPlayers[i].CustomProperties.ContainsKey(PROP_LAP) ? 
                    (int)sortedPlayers[i].CustomProperties[PROP_LAP] : 0);
                slot.UpdateRank(i + 1);
                siblingIndex++;
            }
        }

        foreach (var slot in allSlots)
        {
            if(slot.IsEmpty)
            {
                slot.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }
    }

    // 플레이어가 처음 입장했을 때, 초기 위치와 회전값을 보고하는 함수
    public void ReportAndInitializePlayerInitialPos(GameObject playerObj)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add(PROP_INIT_X, playerObj.transform.position.x);
        props.Add(PROP_INIT_Y, playerObj.transform.position.y);
        props.Add(PROP_INIT_Z, playerObj.transform.position.z);
        props.Add(PROP_INIT_ROT_Y, playerObj.transform.eulerAngles.y);

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // 플레이어가 체크포인트 트리거에 닿았을 때, 해당 체크포인트가 자신의 다음 목표인지 확인하고 진행도 업데이트
    public void ProcessLocalPlayerCheckpointTrigger(GameObject playerObj, Transform cpTransform)
    {
        if (checkpoints.Count == 0) return;

        Player player = PhotonNetwork.LocalPlayer;
        int expectedIndex = player.CustomProperties.ContainsKey(PROP_GOAL) ? (int)player.CustomProperties[PROP_GOAL] : 0;
        if (expectedIndex >= checkpoints.Count) expectedIndex = 0;

        // 닿은 체크포인트가 내 다음 목표
        if (cpTransform == checkpoints[expectedIndex])
        {
            int currentProgress = player.CustomProperties.ContainsKey(PROP_PROG) ? (int)player.CustomProperties[PROP_PROG] : 0;

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            int nextGoalIndex = expectedIndex + 1;

            props.Add(PROP_PROG, currentProgress + 1);
            props.Add(PROP_GOAL, nextGoalIndex);

            // 부활 지점 업데이트 (마지막으로 통과한 체크포인트 위치)
            props.Add(PROP_LAST_X, cpTransform.position.x);
            props.Add(PROP_LAST_Y, cpTransform.position.y);
            props.Add(PROP_LAST_Z, cpTransform.position.z);
            props.Add(PROP_LAST_ROT_Y, cpTransform.eulerAngles.y);

            // 랩 완주 판정
            if (nextGoalIndex >= checkpoints.Count)
            {
                int currentLap = player.CustomProperties.ContainsKey(PROP_LAP) ? (int)player.CustomProperties[PROP_LAP] : 0;
                props[PROP_LAP] = currentLap + 1;
                props[PROP_GOAL] = 0;

                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                TeleportPlayerToInitialPos(playerObj, player);
            }
            else
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
    }

    private void TeleportPlayerToInitialPos(GameObject playerObj, Player targetPlayer)
    {
        if (targetPlayer.CustomProperties.ContainsKey(PROP_INIT_X))
        {
            float x = (float)targetPlayer.CustomProperties[PROP_INIT_X];
            float y = (float)targetPlayer.CustomProperties[PROP_INIT_Y];
            float z = (float)targetPlayer.CustomProperties[PROP_INIT_Z];
            float rotY = (float)targetPlayer.CustomProperties[PROP_INIT_ROT_Y];
            
            TeleportCharacter(playerObj, new Vector3(x, y, z), Quaternion.Euler(0, rotY, 0));
        }
    }

    public void TeleportCharacter(GameObject playerObj, Vector3 pos, Quaternion rot)
    {
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObj.transform.position = pos;
        playerObj.transform.rotation = rot;

        if (cc != null) cc.enabled = true;

        playerObj.GetComponent<PhotonView>().RPC("RPC_SizeReset", RpcTarget.All);
    }

    // 플레이어가 결승선(Trigger)에 닿았을 때 호출
    //public void OnPlayerReachedFinish(GameObject player)
    //{
    //    if (currentState == GameState.Finish) return;

    //    // 포톤 사용 시: 내 캐릭터가 닿았는지 확인
    //    PhotonView pv = player.GetComponent<PhotonView>();

    //    if (pv != null && pv.IsMine)
    //    {
    //        int currentLap = 0;

    //        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
    //        {
    //            currentLap = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
    //        }

    //        currentLap++; // 바퀴 수 1 증가!

    //        // 3. 포톤 서버에 내 바퀴 수 업데이트 (이걸 해야 남들 화면에서도 내 점수가 올라감)
    //        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
    //        props.Add("Score", currentLap);
    //        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

    //        Debug.Log($"{currentLap} 바퀴 통과!");

    //        // 4. 만약 5바퀴를 다 돌았다면 그제서야 진짜 게임 종료
    //        if (currentLap >= 3)
    //        {
    //            FinishGame();
    //        }
    //    }
    //}

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Score(바퀴 수)나 Progress(진행도) 중 하나라도 변경되면 UI 업데이트
        if (changedProps.ContainsKey(PROP_LAP) || changedProps.ContainsKey(PROP_PROG))
        {
            SortPlayerUI();
        }

        if (changedProps.ContainsKey(PROP_LAP))
        {
            int currentLap = (int)changedProps[PROP_LAP];

            if (currentLap >= maxLap)
            {
                OnPlayerFinished();
            }
        }
    }

    public void OpenSystemMenu() { if (systemMenuPanel != null && UIManager.Instance != null) UIManager.Instance.ShowPanel(systemMenuPanel, CloseSystemMenu); }
    private void CloseSystemMenu() { if (systemMenuPanel != null) systemMenuPanel.SetActive(false); }
    private void LeaveRoom() { PhotonNetwork.LeaveRoom(); }
    public override void OnLeftRoom() { SceneManager.LoadScene("Lobby"); }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (activePlayerSlots.ContainsKey(otherPlayer.ActorNumber))
        {
            activePlayerSlots[otherPlayer.ActorNumber].SetEmpty();
            activePlayerSlots.Remove(otherPlayer.ActorNumber);
            SortPlayerUI();
        }
    }

    // 부모에서 abstract로 선언했을 경우 override 사용
    protected override void CheckFinish()
    {
        // 트리거 방식을 쓸 때는 Update에서 매번 체크할 필요가 없으므로 
        // 이 함수는 비워두거나, 다른 보조 판정용으로 씁니다.
    }

    public void OnPlayerFinished()
    {
        if (currentState == GameState.Finish) return;

        // BaseGameManager에 있는 게임 종료 로직 실행
        FinishGame();
    }
}