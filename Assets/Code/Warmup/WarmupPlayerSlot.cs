using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class WarmupPlayerSlot : MonoBehaviour
{
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public GameObject crownIcon;
    public Button optionButton;

    private Player myPlayer;
    private WarmupManager warmupManager;

    public void SetEmpty()
    {
        myPlayer = null;
        backgroundImage.color = Color.gray;
        nameText.text = "";
        crownIcon.SetActive(false);
        optionButton.gameObject.SetActive(false);
    }

    public void Setup(Player player, WarmupManager manager)
    {
        myPlayer = player;
        warmupManager = manager;
        backgroundImage.color = Color.white;
        nameText.text = player.NickName;

        if (player.IsMasterClient)
        {
            crownIcon.SetActive(true);
            optionButton.gameObject.SetActive(false);
        }
        else
        {
            crownIcon.SetActive(false);
            optionButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        optionButton.onClick.RemoveAllListeners();
        optionButton.onClick.AddListener(OnOptionButtonClicked);
    }

    private void OnOptionButtonClicked()
    {
        warmupManager.OpenHostOptionPanel(myPlayer, Input.mousePosition);
    }
}
