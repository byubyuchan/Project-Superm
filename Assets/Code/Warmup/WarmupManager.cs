using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.EventSystems;

public class WarmupManager : MonoBehaviourPunCallbacks
{
    [Header("Player List UI")]
    public GameObject playerListWindow;
    public Transform playerListPanel;
    public GameObject playerSlotPrefab;
    public TextMeshProUGUI playerCountText;
    public Button openPlayerListButton;
    public Button closePlayerListButton;

    [Header("Game Start UI")]
    public Button startButton;
    public TextMeshProUGUI countdownText;

    [Header("Host Option Popup")]
    public GameObject hostOptionPanel;
    public TextMeshProUGUI targetNameText;
    public Button promoteButton;
    public Button kickButton;

    [Header("Room Settings UI")]
    public GameObject roomSettingsPanel;
    public TMP_InputField settingsNameInput;
    public TMP_Dropdown settingsModeDropdown;
    public TMP_InputField settingsMaxPlayersInput;
    public Toggle settingsPrivateToggle;
    public TMP_InputField settingsPasswordInput;
    public Button applySettingsButton;

    [Header("Success Panel UI")]
    public GameObject successPanel;

    [Header("Warning Panel UI")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;

    [Header("System Menu UI")]
    public GameObject systemMenuPanel;
    public Button leaveRoomButton;
    public Button cancelButton;

    private Player targetPlayer;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            ResetRaceProperties();
        }

        hostOptionPanel.SetActive(false);

        if (PhotonNetwork.CurrentRoom != null)
        {
            UpdatePlayerList();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(StartGame);
            OpenRoomSettingsPanel();
            OpenPlayerList();
        }
        else
        {
            startButton.gameObject.SetActive(false);
            CloseRoomSettingsPanel();
            ClosePlayerList();
        }

        promoteButton.onClick.AddListener(DelegateHost);
        kickButton.onClick.AddListener(KickPlayer);

        if (settingsPrivateToggle != null)
        {
            settingsPrivateToggle.onValueChanged.AddListener((isOn) =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    settingsPasswordInput.interactable = isOn;
                    if (!isOn) settingsPasswordInput.text = "";
                }
            });
        }

        if (successPanel != null) successPanel.SetActive(false);
        if (warningPanel != null) warningPanel.SetActive(false);
        if (systemMenuPanel != null) systemMenuPanel.SetActive(false);
        if (leaveRoomButton != null) leaveRoomButton.onClick.AddListener(LeaveRoom);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseSystemMenu);

        TMP_InputField[] allInputs = FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var input in allInputs)
        {
            input.restoreOriginalTextOnEscape = false;
        }

        UIManager.Instance.onEmptyEsc = OpenSystemMenu;

        if (openPlayerListButton != null) openPlayerListButton.onClick.AddListener(OpenPlayerList);
        if (closePlayerListButton != null) closePlayerListButton.onClick.AddListener(ClosePlayerList);
    }

    public void OpenPlayerList()
    {
        if (playerListWindow != null) UIManager.Instance.ShowPanel(playerListWindow, ClosePlayerList);
        if (openPlayerListButton != null) openPlayerListButton.gameObject.SetActive(false);
    }

    public void ClosePlayerList()
    {
        if (playerListWindow != null) playerListWindow.SetActive(false);
        if (openPlayerListButton != null) openPlayerListButton.gameObject.SetActive(true);
    }

    private void ResetRaceProperties()
    {
        if(PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable resetProps = new ExitGames.Client.Photon.Hashtable();

            resetProps.Add(RunGameManager.PROP_LAP, 0);
            resetProps.Add(RunGameManager.PROP_PROG, 0);
            resetProps.Add(RunGameManager.PROP_GOAL, 0);

            resetProps.Add(RunGameManager.PROP_INIT_X, null);
            resetProps.Add(RunGameManager.PROP_INIT_Y, null);
            resetProps.Add(RunGameManager.PROP_INIT_Z, null);
            resetProps.Add(RunGameManager.PROP_INIT_ROT_Y, null);

            resetProps.Add(RunGameManager.PROP_LAST_X, null);
            resetProps.Add(RunGameManager.PROP_LAST_Y, null);
            resetProps.Add(RunGameManager.PROP_LAST_Z, null);
            resetProps.Add(RunGameManager.PROP_LAST_ROT_Y, null);

            PhotonNetwork.LocalPlayer.SetCustomProperties(resetProps);
        }
    }

    void Update()
    {
        if (hostOptionPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            RectTransform panelRect = hostOptionPanel.GetComponent<RectTransform>();
            if(!RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition))
            {
                CloseHostOptionPanel();
            }
        }
    }

    private void UpdatePlayerList()
    {
        foreach (Transform child in playerListPanel)
        {
            Destroy(child.gameObject);
        }

        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        Player[] rawPlayers = PhotonNetwork.PlayerList;

        List<Player> sortedPlayers = new List<Player>();
        foreach (Player p in rawPlayers)
        {
            if (p.IsMasterClient)
            {
                sortedPlayers.Insert(0, p);
            }
            else
            {
                sortedPlayers.Add(p);
            }
        }

        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject slotObj = Instantiate(playerSlotPrefab, playerListPanel);
            WarmupPlayerSlot slot = slotObj.GetComponent<WarmupPlayerSlot>();

            if (i < sortedPlayers.Count)
            {
                slot.Setup(sortedPlayers[i], this);
            }
            else
            {
                slot.SetEmpty();
            }
        }

        playerCountText.text = $"{sortedPlayers.Count} / {maxPlayers}";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();

        if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                StartGame();
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();

        if(PhotonNetwork.IsMasterClient && !startButton.gameObject.activeSelf)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        if(targetPlayer == otherPlayer)
        {
            CloseHostOptionPanel();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdatePlayerList();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
    }

    public void OpenHostOptionPanel(Player player, Vector3 mousePos)
    {
        targetPlayer = player;
        targetNameText.text = player.NickName;
        hostOptionPanel.transform.position = mousePos;
        UIManager.Instance.ShowPanel(hostOptionPanel, CloseHostOptionPanel);
    }

    private void CloseHostOptionPanel()
    {
        hostOptionPanel.SetActive(false);
        targetPlayer = null;
    }

    private void DelegateHost()
    {
        if (targetPlayer != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.SetMasterClient(targetPlayer);
            CloseHostOptionPanel();
        }
    }

    private void KickPlayer()
    {
        if (targetPlayer != null && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_Kicked", targetPlayer);
            CloseHostOptionPanel();
        }
    }

    [PunRPC]
    private void RPC_Kicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        UpdatePlayerList();
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props.Add("CharacterType", "Warrior");
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // IsOpen은 로비 방에는 여전히 뜨지만 다른 사람의 입장을 거부하는 설정
            PhotonNetwork.CurrentRoom.IsOpen = false;
            // IsVisible은 방이 로비에서 보이지 않도록 설정
            PhotonNetwork.CurrentRoom.IsVisible = false;
            photonView.RPC("RPC_StartCountdown", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_StartCountdown()
    {
        startButton.gameObject.SetActive(false);
        StartCoroutine(CountdownCoroutine());
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        countdownText.gameObject.SetActive(true);

        int count = 5;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "START!";
        yield return new WaitForSeconds(1f);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("RunScene");
        }
    }

    public void OpenRoomSettingsPanel()
    {
        UIManager.Instance.ShowPanel(roomSettingsPanel, CloseRoomSettingsPanel);
        Room room = PhotonNetwork.CurrentRoom;

        settingsNameInput.text = room.Name;
        settingsMaxPlayersInput.text = room.MaxPlayers.ToString();

        Hashtable cp = room.CustomProperties;
        if(cp.ContainsKey("roomName")) settingsNameInput.text = cp["roomName"].ToString();
        if (cp.ContainsKey("mode"))
        {
            string currentMode = cp["mode"].ToString();
            int index = settingsModeDropdown.options.FindIndex(o => o.text == currentMode);
            if (index >= 0) settingsModeDropdown.value = index;
        }
        if (cp.ContainsKey("isPrivate")) settingsPrivateToggle.isOn = (bool)cp["isPrivate"];
        if (cp.ContainsKey("password")) settingsPasswordInput.text = cp["password"].ToString();

        bool isHost = PhotonNetwork.IsMasterClient;

        settingsNameInput.interactable = isHost;
        settingsModeDropdown.interactable = isHost;
        settingsMaxPlayersInput.interactable = isHost;
        settingsPrivateToggle.interactable = isHost;
        settingsPasswordInput.interactable = isHost && settingsPrivateToggle.isOn;

        applySettingsButton.gameObject.SetActive(isHost);
    }

    public void CloseRoomSettingsPanel()
    {
        roomSettingsPanel.SetActive(false);
    }

    public void ApplyRoomSettings()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Room room = PhotonNetwork.CurrentRoom;

        if (string.IsNullOrWhiteSpace(settingsNameInput.text))
        { 
            ShowWarning("Please enter a valid room name");
            return;
        }

        int max = 0;
        if (!int.TryParse(settingsMaxPlayersInput.text, out max) || max < 2 || max > 8)
        {
            ShowWarning("Players must be a number between 2 and 8");
            return;
        }

        if (max < room.PlayerCount)
        {
            ShowWarning($"Cannot set max players below current player count ({room.PlayerCount})");
            return;
        }

        if (settingsPrivateToggle != null && settingsPrivateToggle.isOn)
        {
            if (string.IsNullOrWhiteSpace(settingsPasswordInput.text) ||
                settingsPasswordInput.text.Length < 1 ||
                settingsPasswordInput.text.Length > 8)
            {
                ShowWarning("Please enter a valid password (1-8 characters)");
                return;
            }
        }

        room.MaxPlayers = (byte)max;

        Hashtable cp = new Hashtable();
        cp["roomName"] = settingsNameInput.text;
        cp["mode"] = settingsModeDropdown.options[settingsModeDropdown.value].text;
        cp["isPrivate"] = settingsPrivateToggle.isOn;
        cp["password"] = settingsPasswordInput.text;

        room.SetCustomProperties(cp);

        ShowSuccessMessage();
    }

    private void ShowSuccessMessage()
    {
        if (successPanel != null) UIManager.Instance.ShowPanel(successPanel, CloseSuccessPanel);
    }

    public void CloseSuccessPanel()
    {
        if (successPanel != null) successPanel.SetActive(false);
    }

    private void ShowWarning(string message)
    {
        if (warningText != null) warningText.text = message;
        if (warningPanel != null) UIManager.Instance.ShowPanel(warningPanel, CloseWarningPanel);
    }

    public void CloseWarningPanel()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
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

    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 완료: 속성 초기화 및 리스트 업데이트");
        ResetRaceProperties();
        UpdatePlayerList();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            OpenRoomSettingsPanel();
            OpenPlayerList();
        }
        else
        {
            ClosePlayerList();
        }
    }
}
