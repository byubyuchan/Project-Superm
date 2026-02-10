using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Main Lobby UI")]
    public GameObject lobbyPanel;
    public Toggle availableRoomToggle;
    public TMP_InputField searchInput;
    public Button prevPageButton;
    public Button nextPageButton;

    [Header("Create Room Panel UI")]
    public GameObject createRoomPanel;
    public TMP_InputField roomNameInput;
    public TMP_Dropdown modeDropdown;
    public TMP_InputField maxPlayersInput;
    public Toggle privateToggle;
    public TMP_InputField passwordInput;

    [Header("Join Room UI")]
    public GameObject passwordPanel;
    public TMP_InputField joinPasswordInput;

    [Header("Room List UI")]
    public RoomSlot[] roomSlots;
    public TextMeshProUGUI pageText;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    private RoomInfo currentTargetRoom;
    private int currentPage = 0;
    private int itemsPerPage;

    private void Start()
    {
        itemsPerPage = roomSlots.Length;

        if (availableRoomToggle != null)
        {
            availableRoomToggle.isOn = false;
            availableRoomToggle.onValueChanged.AddListener(OnAvailableToggleChange);
        }

        if (searchInput != null) searchInput.onValueChanged.AddListener(OnSearchValueChange);
        if (prevPageButton != null) prevPageButton.onClick.AddListener(PrevPage);
        if (nextPageButton != null) nextPageButton.onClick.AddListener(NextPage);

        if (privateToggle != null)
        {
            privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
            OnPrivateToggleChanged(privateToggle.isOn);
        }

        Debug.Log("포톤 서버 접속 시도...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OpenCreateRoomPanel()
    {
        createRoomPanel.SetActive(true);

        roomNameInput.text = "";
        if (modeDropdown != null) modeDropdown.value = 0;
        maxPlayersInput.text = "4";

        if (privateToggle != null) privateToggle.isOn = false;
        passwordInput.text = "";
    }

    public void CloseCreateRoomPanel()
    {
        createRoomPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료! 로비 진입 요청...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료! 방 목록 대기 중...");
        cachedRoomList.Clear();
        UpdateRoomListUI();
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text)) return;

        RoomOptions roomOptions = new RoomOptions();

        byte maxPlayers = 4;
        if (int.TryParse(maxPlayersInput.text, out int max)) maxPlayers = (byte)max;
        roomOptions.MaxPlayers = maxPlayers;

        Hashtable cp = new Hashtable();
        cp["mode"] = modeDropdown.options[modeDropdown.value].text;
        cp["isPrivate"] = privateToggle.isOn;
        cp["password"] = passwordInput.text;

        roomOptions.CustomRoomProperties = cp;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "mode", "isPrivate" };

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공!");
        CloseCreateRoomPanel();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 완료 (로비 화면 유지)");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var info in roomList)
        {
            int index = cachedRoomList.FindIndex(x => x.Name == info.Name);
            if (index != -1) cachedRoomList.RemoveAt(index);

            if (!info.RemovedFromList)
            {
                cachedRoomList.Add(info);
            }
        }
        UpdateRoomListUI();
    }

    void UpdateRoomListUI()
    {
        List<RoomInfo> filteredList = new List<RoomInfo>();
        string searchText = (searchInput != null) ? searchInput.text.ToLower() : "";
        bool showAvailableOnly = (availableRoomToggle != null && availableRoomToggle.isOn);

        foreach (var room in cachedRoomList)
        {
            bool matchSearch = string.IsNullOrEmpty(searchText) || room.Name.ToLower().Contains(searchText);

            bool matchAvailable = true;
            bool isPrivate = false;
            if (room.CustomProperties.ContainsKey("isPrivate"))
                isPrivate = (bool)room.CustomProperties["isPrivate"];

            if (showAvailableOnly)
            {
                if (room.PlayerCount >= room.MaxPlayers || isPrivate) matchAvailable = false;
            }

            if (matchSearch && matchAvailable)
            {
                filteredList.Add(room);
            }
        }

        int totalPages = Mathf.CeilToInt((float)filteredList.Count / itemsPerPage);
        if (totalPages == 0) totalPages = 1;
        if (currentPage >= totalPages) currentPage = totalPages - 1;
        if (currentPage < 0) currentPage = 0;

        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < roomSlots.Length; i++)
        {
            int dataIndex = startIndex + i;
            roomSlots[i].gameObject.SetActive(true);

            if (dataIndex < filteredList.Count)
            {
                RoomInfo info = filteredList[dataIndex];
                roomSlots[i].SetInfo(info);

                Button btn = roomSlots[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnSlotClicked(info));
            }
            else
            {
                roomSlots[i].ClearSlot();
                roomSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }

        pageText.text = $"{currentPage + 1} / {totalPages}";
        if (prevPageButton != null) prevPageButton.interactable = (currentPage > 0);
        if (nextPageButton != null) nextPageButton.interactable = (currentPage < totalPages - 1);
    }

    public void OnSlotClicked(RoomInfo room)
    {
        bool isPrivate = false;
        if (room.CustomProperties.ContainsKey("isPrivate"))
            isPrivate = (bool)room.CustomProperties["isPrivate"];

        if (isPrivate)
        {
            currentTargetRoom = room;
            OpenPasswordPanel();
        }
        else
        {
            PhotonNetwork.JoinRoom(room.Name);
        }
    }

    public void SubmitPassword()
    {
        if (currentTargetRoom == null) return;

        string serverPass = "";
        if (currentTargetRoom.CustomProperties.ContainsKey("password"))
            serverPass = (string)currentTargetRoom.CustomProperties["password"];

        if (joinPasswordInput.text == serverPass)
        {
            ClosePasswordPanel();
            PhotonNetwork.JoinRoom(currentTargetRoom.Name);
        }
        else
        {
            Debug.LogWarning("비밀번호 불일치");
            joinPasswordInput.text = "";
        }
    }

    // Helper Functions
    public void OnPrivateToggleChanged(bool isOn) { passwordInput.interactable = isOn; if (!isOn) passwordInput.text = ""; }
    public void OnSearchValueChange(string text) { currentPage = 0; UpdateRoomListUI(); }
    public void OnAvailableToggleChange(bool isOn) { currentPage = 0; UpdateRoomListUI(); }
    public void NextPage() { currentPage++; UpdateRoomListUI(); }
    public void PrevPage() { currentPage--; UpdateRoomListUI(); }
    void OpenPasswordPanel() { passwordPanel.SetActive(true); joinPasswordInput.text = ""; joinPasswordInput.Select(); }
    public void ClosePasswordPanel() { passwordPanel.SetActive(false); currentTargetRoom = null; }
}