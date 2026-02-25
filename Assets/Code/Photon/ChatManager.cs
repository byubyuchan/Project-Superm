using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ChatManager : MonoBehaviourPunCallbacks
{
    [Header("Chat UI")]
    public TMP_InputField chatInput;
    public Transform content;
    public GameObject chatPrefab;
    public ScrollRect chatScroolRect;

    [Header("Chat Backgrounds")]
    public Image chatPanelImage;
    public Image scrollViewImage;

    private bool isChatActive = false;

    void Start()
    {
        SetChatUIActive(false);
        AddSystemMessage("You have joined the room.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatActive)
            {
                SetChatUIActive(true);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(chatInput.text))
                {
                    SendChatMessage();
                }

                SetChatUIActive(false);
            }
        }
    }

    private void SetChatUIActive(bool isActive)
    {
        isChatActive = isActive;

        chatInput.gameObject.SetActive(isActive);

        if (chatPanelImage != null) chatPanelImage.enabled = isActive;
        if (scrollViewImage != null) scrollViewImage.enabled = isActive;

        if (isActive)
        {
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
        else
        {
            chatInput.text = "";
        }
    }

    public void SendChatMessage()
    {
        string message = chatInput.text;

        string senderName = PhotonNetwork.NickName;

        photonView.RPC("ReceiveMessage", RpcTarget.All, senderName, message);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    [PunRPC]
    public void ReceiveMessage(string senderName, string message)
    {
        GameObject newChat = Instantiate(chatPrefab, content);

        TextMeshProUGUI chatText = newChat.GetComponent<TextMeshProUGUI>();

        chatText.text = $"[{senderName}]: {message}";

        StartCoroutine(ScrollToBottom());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddSystemMessage($"{newPlayer.NickName} has joined the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        AddSystemMessage($"{otherPlayer.NickName} has left the room.");
    }

    private void AddSystemMessage(string message)
    {
        GameObject newChat = Instantiate(chatPrefab, content);
        TextMeshProUGUI chatText = newChat.GetComponent<TextMeshProUGUI>();

        chatText.text = $"<color=yellow>{message}</color>";

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return null; // Wait for the end of the frame to text to calculate its size
        yield return null; // Wait another frame to content size to expand its size

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        chatScroolRect.verticalNormalizedPosition = 0f;
    }
}