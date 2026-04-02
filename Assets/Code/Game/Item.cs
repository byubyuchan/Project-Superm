using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class Item : MonoBehaviourPun
{
    [SerializeField]
    public ItemData[] dataSet;

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
            int randomIndex = Random.Range(0, dataSet.Length);

            playerPV.RPC("RPC_GetItem", playerPV.Owner, dataSet[randomIndex].name);
        }
    }
}
// 차후 랜덤한 아이템 획득으로 바뀐 후 UI와 플레이어에게 할당해 캔버스에서 보이게하고 MoveByKeys에서 Use하며 RPC도 같이 호출하는 방식으로 바꿔야할듯. 어차피 플레이어들에게만 작동하니까