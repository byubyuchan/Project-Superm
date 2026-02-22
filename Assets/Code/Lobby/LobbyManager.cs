using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public Button showCreateRoomPasswordButton;
    public Image showCreateRoomPasswordImage;
    private bool isCreateRoomPasswordVisible = true;

    [Header("Password Panel UI")]
    public Button showPasswordButton;
    public Image showPasswordImage;
    private bool isPasswordVisible = true;

    [Header("Eye Sprite")]
    public Sprite eyeOpenSprite;
    public Sprite eyeClosedSprite;

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
        PhotonNetwork.AutomaticallySyncScene = true;

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

        if (showCreateRoomPasswordButton != null)
            showCreateRoomPasswordButton.onClick.AddListener(ToggleCreateRoomPasswordVisibility);

        if (showPasswordButton != null)
            showPasswordButton.onClick.AddListener(TogglePasswordVisibility);

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

        isCreateRoomPasswordVisible = true;
        passwordInput.contentType = TMP_InputField.ContentType.Standard;
        if (showCreateRoomPasswordImage != null) showCreateRoomPasswordImage.sprite = eyeOpenSprite;
        passwordInput.ForceLabelUpdate();
    }

    public void ToggleCreateRoomPasswordVisibility()
    {
        isCreateRoomPasswordVisible = !isCreateRoomPasswordVisible;

        if (isCreateRoomPasswordVisible)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            showCreateRoomPasswordImage.sprite = eyeOpenSprite;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            showCreateRoomPasswordImage.sprite = eyeClosedSprite;
        }

        passwordInput.ForceLabelUpdate();
    }

    public void CloseCreateRoomPanel()
    {
        createRoomPanel.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
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
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "mode", "isPrivate", "password" };

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공");
        CloseCreateRoomPanel();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("RunScene");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공(지금은 로비 씬 유지)");

        //SceneManager.LoadScene("RunScene");

        //PhotonNetwork.LoadLevel("RunScene");
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
        if (currentTargetRoom == null || joinPasswordInput == null) return;

        string serverPass = "";
        if (currentTargetRoom.CustomProperties != null && currentTargetRoom.CustomProperties.ContainsKey("password"))
        {
            object val = currentTargetRoom.CustomProperties["password"];
            if (val != null) serverPass = val.ToString();
        }

        if (joinPasswordInput.text == serverPass)
        {
            PhotonNetwork.JoinRoom(currentTargetRoom.Name);

            ClosePasswordPanel();
        }
        else
        {
            Debug.LogWarning("비밀번호 불일치");
            joinPasswordInput.text = "";
            joinPasswordInput.Select();
        }
    }

    public void OnPrivateToggleChanged(bool isOn)
    {
        if (passwordInput != null)
        {
            passwordInput.interactable = isOn;
            if (!isOn) passwordInput.text = "";
        }
    }
    public void OnSearchValueChange(string text) { currentPage = 0; UpdateRoomListUI(); }
    public void OnAvailableToggleChange(bool isOn) { currentPage = 0; UpdateRoomListUI(); }
    public void NextPage() { currentPage++; UpdateRoomListUI(); }
    public void PrevPage() { currentPage--; UpdateRoomListUI(); }

    void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        joinPasswordInput.text = "";
        joinPasswordInput.Select();

        isPasswordVisible = true;
        joinPasswordInput.contentType = TMP_InputField.ContentType.Standard;
        if (showPasswordImage != null) showPasswordImage.sprite = eyeOpenSprite;
        joinPasswordInput.ForceLabelUpdate();
    }

    private void TogglePasswordVisibility()
    {
        isPasswordVisible = !isPasswordVisible;

        if (isPasswordVisible)
        {
            joinPasswordInput.contentType = TMP_InputField.ContentType.Standard;
            showPasswordImage.sprite = eyeOpenSprite;
        }
        else
        {
            joinPasswordInput.contentType = TMP_InputField.ContentType.Password;
            showPasswordImage.sprite = eyeClosedSprite;
        }

        joinPasswordInput.ForceLabelUpdate();
    }

    public void ClosePasswordPanel()
    {
        passwordPanel.SetActive(false);
        currentTargetRoom = null;
    }
}