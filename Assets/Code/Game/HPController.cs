using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class HPController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float Hp = 1f;
    private float maxHp;

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

    // ?? 내 캐릭터의 HP가 변하면 네트워크를 통해 다른 사람들에게 실시간 스트리밍합니다.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내 캐릭터라면 내 현재 HP를 보냄
            stream.SendNext(this.Hp);
        }
        else
        {
            // 다른 사람 캐릭터라면 그 사람의 HP를 받아와서 내 화면에 갱신
            this.Hp = (float)stream.ReceiveNext();
        }
    }
}