using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ChatManager : MonoBehaviourPunCallbacks
{
    public GameObject m_Content;
    public TMP_InputField m_inputField;

    PhotonView photonview;

    GameObject m_ContentText;

    string m_strUserName;


    void Start()
    {
        m_ContentText = m_Content.transform.GetChild(0).gameObject;
        photonview = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && m_inputField.isFocused == false)
        {
            m_inputField.ActivateInputField();
        }
    }

    public override void OnJoinedRoom()
    {
        AddChatMessage("connect user : " + PhotonNetwork.LocalPlayer.NickName);

        m_strUserName = PhotonNetwork.LocalPlayer.NickName;
    }

    public void OnEndEditEvent()
    {
        Debug.Log("OnEndEditEvent");
        string strMessage = m_strUserName + " : " + m_inputField.text;

        photonview.RPC("RPC_Chat", RpcTarget.All, strMessage);
        m_inputField.text = "";
    }

    public void AddChatMessage(string message)
    {
        GameObject goText = Instantiate(m_ContentText, m_Content.transform);

        goText.GetComponent<TextMeshProUGUI>().text = message;
        m_Content.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

    }

    [PunRPC]
    public void RPC_Chat(string message)
    {
        AddChatMessage(message);
    }

}