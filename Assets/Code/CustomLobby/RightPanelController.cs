using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class RightPanelController : MonoBehaviour
{
    [Header("Top Info")]
    public TextMeshProUGUI playerNameText;

    [Header("Tab Buttons")]
    public Button chatButton;
    public Button friendButton;
    public Button userButton;

    private void Start()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PhotonNetwork.NickName;
        }

        OnChatButtonClicked();
    }

    public void OnChatButtonClicked()
    {
        if (chatButton != null) chatButton.interactable = false;
        if (friendButton != null) friendButton.interactable = true;
        if (userButton != null) userButton.interactable = true;
    }

    public void OnFriendButtonClicked()
    {
        if (chatButton != null) chatButton.interactable = true;
        if (friendButton != null) friendButton.interactable = false;
        if (userButton != null) userButton.interactable = true;
    }

    public void OnUserButtonClicked()
    {
        if (chatButton != null) chatButton.interactable = true;
        if (friendButton != null) friendButton.interactable = true;
        if (userButton != null) userButton.interactable = false;
    }
}