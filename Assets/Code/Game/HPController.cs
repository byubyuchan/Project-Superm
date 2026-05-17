using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using UnityEngine;

public class HPController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float Hp = 100f;
    public float maxHp;

    [SerializeField]
    private GameObject UIprefab;

    [SerializeField]
    private float respawnDelay = 3f;

    public bool isDead = false;
    

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
    public void Die()
    {
        isDead = true;
        RespawnPlayer(respawnDelay);
    }

    public void RespawnPlayer(float delay)
    {
        if (photonView.IsMine)
        {
            StartCoroutine(PlayerRespawnRoutine(delay));
        }
    }

    IEnumerator PlayerRespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        BaseGameManager manager = Object.FindFirstObjectByType<BaseGameManager>();

        if (manager != null)
        {
            manager.RequestTeleport(gameObject);
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