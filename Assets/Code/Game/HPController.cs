using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class HPController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float Hp = 100f;
    public float maxHp;

    [SerializeField]
    private GameObject UIprefab;

    private void Start()
    {
        maxHp = Hp;

        if (this.UIprefab != null)
        {
            GameObject _uiGo = Instantiate(this.UIprefab, Vector3.zero, Quaternion.identity);

            PlayerUI playerUI = _uiGo.GetComponent<PlayerUI>();
            if (playerUI != null)
            {
                playerUI.SetTarget(this);
            }
        }
    }

    // Photon Update()
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.Hp);
        }
        else
        {
            this.Hp = (float)stream.ReceiveNext();
        }
    }
}