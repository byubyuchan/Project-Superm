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

    private float originY;

    private void Start()
    {
        if (photonView.IsMine) Invoke("DestroySelf", respawnTime);
        originY = transform.position.y;
    }

    void FixedUpdate()
    {
        // 아이템 스폰은 15f 높이로 고정이기에 하드하게 코딩했음. 서로를 참조받는 형식보다 빠름.
        float transformY = originY - transform.position.y;

        if (transformY >= 15f && !GetComponent<Rigidbody>().isKinematic)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            transform.position = new Vector3(transform.position.x, originY - 15f, transform.position.z);
        }
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