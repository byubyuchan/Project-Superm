using UnityEngine;
using TMPro;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbySocialManager : MonoBehaviour, IChatClientListener
{
    [Header("Chat UI")]
    public TMP_InputField chatInput;
    public GameObject chatTextPrefab;
    public Transform chatContent;
    public ScrollRect chatScrollRect;

    [Header("User List UI")]
    public Transform userContent;
    public GameObject userSlotPrefab;

    private ChatClient chatClient;
    private string lobbyChannelName = "Global_Lobby";

    private void Start()
    {
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "1.0", new AuthenticationValues(PhotonNetwork.NickName));

        if (chatInput != null)
        {
            chatInput.onSubmit.AddListener(OnSubmitChat);
        }
    }

    private void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }
    }

    private void OnSubmitChat(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            StartCoroutine(RefocusInputField());
            return;
        }

        chatClient.PublishMessage(lobbyChannelName, text);

        chatInput.text = "";

        StartCoroutine(RefocusInputField());
    }

    private IEnumerator RefocusInputField()
    {
        yield return null;
        chatInput.ActivateInputField();
    }

    public void OnConnected()
    {
        ChannelCreationOptions options = new ChannelCreationOptions();
        options.PublishSubscribers = true;

        chatClient.Subscribe(lobbyChannelName, 0, -1, options);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        if (chatClient.TryGetChannel(lobbyChannelName, out ChatChannel channel))
        {
            RefreshUserList(channel.Subscribers);
        }
    }

    private void RefreshUserList(HashSet<string> subscribers)
    {
        foreach (Transform child in userContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string user in subscribers)
        {
            AddUserSlot(user);
        }
    }

    private void AddUserSlot(string userName)
    {
        GameObject slot = Instantiate(userSlotPrefab, userContent);
        slot.name = userName;

        TMP_Text text = slot.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = userName;
        }
    }

    private void RemoveUserSlot(string userName)
    {
        foreach (Transform child in userContent)
        {
            if (child.name == userName)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            GameObject textObj = Instantiate(chatTextPrefab, chatContent);
            TMP_Text chatText = textObj.GetComponent<TMP_Text>();
            chatText.text = $"{senders[i]}: {messages[i]}";

            chatText.ForceMeshUpdate();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return null;

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void OnUserSubscribed(string channel, string user)
    {
        if (channel == lobbyChannelName)
        {
            AddUserSlot(user);
        }
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        if (channel == lobbyChannelName)
        {
            RemoveUserSlot(user);
        }
    }

    public void DebugReturn(DebugLevel level, string message) { }
    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
}