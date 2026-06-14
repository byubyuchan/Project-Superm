using UnityEngine;
using TMPro;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class LobbySocialManager : MonoBehaviour, IChatClientListener
{
    [Header("Chat UI")]
    public TMP_InputField chatInput;
    public GameObject chatTextPrefab;
    public Transform chatContent;
    public ScrollRect chatScrollRect;

    private ChatClient chatClient;
    private string lobbyChannelName = "Global_Lobby";

    private void Start()
    {
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, "1.0", new AuthenticationValues(PhotonNetwork.NickName));
    }

    private void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service();
        }
    }

    public void SendChatMessage()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text)) return;

        chatClient.PublishMessage(lobbyChannelName, chatInput.text);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    public void OnConnected()
    {
        chatClient.Subscribe(new string[] { lobbyChannelName });
    }

    public void OnSubscribed(string[] channels, bool[] results) { }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            GameObject textObj = Instantiate(chatTextPrefab, chatContent);
            TMP_Text chatText = textObj.GetComponent<TMP_Text>();

            chatText.text = $"{senders[i]}: {messages[i]}";
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void DebugReturn(DebugLevel level, string message) { }
    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }
}