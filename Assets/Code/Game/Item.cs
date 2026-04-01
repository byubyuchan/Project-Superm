using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviourPun
{
    [SerializeField]
    private string[] RPC = { "RPC_SizeDown", "RPC_SizeUp", "RPC_Magnet" };

    [Header("Magnet Settings")]
    public float magnetRadius = 200f;
    public float magnetPower = 100f;

    [SerializeField]
    private float respawnTime = 10f;

    private void Start()
    {
        if (photonView.IsMine) Invoke("DestroySelf", respawnTime);
    }

    void DestroySelf()
    {
        if (photonView.IsMine && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ApplyRandomEffect(other.gameObject);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
    void ApplyRandomEffect(GameObject player)
    {
        PhotonView playerPV = player.GetComponent<PhotonView>();
        if (playerPV != null)
        {

            int randomIndex = Random.Range(0, RPC.Length);
            string selectedRPC = RPC[randomIndex];

            if (selectedRPC == "RPC_Magnet")
            {
                playerPV.RPC("RPC_Magnet", RpcTarget.All, magnetRadius, magnetPower);
            }
            else playerPV.RPC(selectedRPC, RpcTarget.All);
        }
    }
}
// 차후 랜덤한 아이템 획득으로 바뀐 후 UI와 플레이어에게 할당해 캔버스에서 보이게하고 MoveByKeys에서 Use하며 RPC도 같이 호출하는 방식으로 바꿔야할듯. 어차피 플레이어들에게만 작동하니까