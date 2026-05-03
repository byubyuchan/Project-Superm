using Photon.Pun;
using UnityEngine;

public class SleepProjectile : Projectile
{
    protected override void OnCollisionEnter(Collision col)
    {
        if (!photonView.IsMine || hasExploded) return;

        if (col.gameObject.CompareTag("Player"))
        {
            Explode(col.gameObject);
        }
        else if (col.gameObject.CompareTag("Map"))
        {
            Explode();
        }
    }

    protected override void Explode(GameObject target = null)
    {
        hasExploded = true;
        CancelInvoke("DestroySelf");

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.RequestExplosion(explosionEffectIndex, transform.position);
        }

        if (target != null && target.CompareTag("Player"))
        {
            PhotonView targetPV = target.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                targetPV.RPC("RPC_Sleep", RpcTarget.All, explosionForce);
            }
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}