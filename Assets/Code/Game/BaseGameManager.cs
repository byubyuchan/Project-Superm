using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class BaseGameManager : MonoBehaviourPunCallbacks
{
    public static class PhotonKeys
    {
        // 게임 데이터 관련
        public const string LAP = "Score";
        public const string PROGRESS = "Progress";
        public const string GOAL = "NextCP";

        // 초기 스폰 위치 관련
        public const string INIT_X = "InitX";
        public const string INIT_Y = "InitY";
        public const string INIT_Z = "InitZ";
        public const string INIT_ROT_Y = "InitRotY";

        // 마지막 체크포인트(부활) 위치 관련
        public const string LAST_X = "LastX";
        public const string LAST_Y = "LastY";
        public const string LAST_Z = "LastZ";
        public const string LAST_ROT_Y = "LastRotY";
    }

    public enum GameState { Wait, Playing, Finish }
    protected GameState currentState = GameState.Wait;

    [Header("GameEnd")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI countdownText;

    [Header("System Menu UI")]
    public GameObject systemMenuPanel;
    public Button leaveRoomButton;
    public Button cancelButton;

    protected void Start()
    {
        Application.targetFrameRate = 240;
        PhotonNetwork.AutomaticallySyncScene = true;

        if (systemMenuPanel != null) systemMenuPanel.SetActive(false);
        if (leaveRoomButton != null) leaveRoomButton.onClick.AddListener(LeaveRoom);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseSystemMenu);

    }

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

    public void OpenSystemMenu()
    {
        if (systemMenuPanel != null)
        {
            UIManager.Instance.ShowPanel(systemMenuPanel, CloseSystemMenu);

            Photon.Pun.UtilityScripts.MoveByKeys[] players =
                FindObjectsByType<Photon.Pun.UtilityScripts.MoveByKeys>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.photonView.IsMine)
                {
                    p.isUIMode = true;
                    p.isMenuOpen = true;
                    break;
                }
            }
        }
    }

    private void CloseSystemMenu()
    {
        if (systemMenuPanel != null)
        {
            systemMenuPanel.SetActive(false);

            Photon.Pun.UtilityScripts.MoveByKeys[] players =
                FindObjectsByType<Photon.Pun.UtilityScripts.MoveByKeys>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p.photonView.IsMine)
                {
                    p.isUIMode = false;
                    p.isMenuOpen = false;
                    break;
                }
            }
        }
    }
    protected void ResetPlayerGameProperties()
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable();

            resetProps.Add(PhotonKeys.LAP, 0);
            resetProps.Add(PhotonKeys.PROGRESS, 0);
            resetProps.Add(PhotonKeys.GOAL, 0);

            // 위치 정보들은 null로 밀어서 초기화
            resetProps.Add(PhotonKeys.INIT_X, null);
            resetProps.Add(PhotonKeys.INIT_Y, null);
            resetProps.Add(PhotonKeys.INIT_Z, null);
            resetProps.Add(PhotonKeys.INIT_ROT_Y, null);

            resetProps.Add(PhotonKeys.LAST_X, null);
            resetProps.Add(PhotonKeys.LAST_Y, null);
            resetProps.Add(PhotonKeys.LAST_Z, null);
            resetProps.Add(PhotonKeys.LAST_ROT_Y, null);

            PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);
        }
    }
    protected void TeleportPlayerToInitialPos(GameObject playerObj, Player targetPlayer)
    {
        if (targetPlayer.CustomProperties.ContainsKey(PhotonKeys.INIT_X))
        {
            float x = (float)targetPlayer.CustomProperties[PhotonKeys.INIT_X];
            float y = (float)targetPlayer.CustomProperties[PhotonKeys.INIT_Y];
            float z = (float)targetPlayer.CustomProperties[PhotonKeys.INIT_Z];
            float rotY = (float)targetPlayer.CustomProperties[PhotonKeys.INIT_ROT_Y];

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

    public bool GetBestRespawnPoint(out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;
        Player p = PhotonNetwork.LocalPlayer;

        // 1. 마지막 체크포인트(Last)가 있는지 확인
        if (p.CustomProperties.ContainsKey(PhotonKeys.LAST_X))
        {
            pos = new Vector3((float)p.CustomProperties[PhotonKeys.LAST_X],
                             (float)p.CustomProperties[PhotonKeys.LAST_Y],
                             (float)p.CustomProperties[PhotonKeys.LAST_Z]);
            rot = Quaternion.Euler(0, (float)p.CustomProperties[PhotonKeys.LAST_ROT_Y], 0);
            return true;
        }

        // 2. 없으면 초기 시작 위치(Init) 확인
        if (p.CustomProperties.ContainsKey(PhotonKeys.INIT_X))
        {
            pos = new Vector3((float)p.CustomProperties[PhotonKeys.INIT_X],
                             (float)p.CustomProperties[PhotonKeys.INIT_Y],
                             (float)p.CustomProperties[PhotonKeys.INIT_Z]);
            rot = Quaternion.Euler(0, (float)p.CustomProperties[PhotonKeys.INIT_ROT_Y], 0);
            return true;
        }

        // 차후, false는 배틀로얄 모드에서 사용하면 좋을듯!! 스폰 위치를 랜덤으로 지정해서 부활하도록?? 

        return false;
    }

    public virtual void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom(); 
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }


}