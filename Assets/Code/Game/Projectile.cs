using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Projectile : MonoBehaviourPun
{
    public float speed = 20f;
    public float lifeTime = 3f;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;   // 폭발 반경
    public float explosionForce = 15f;    // 밀어내는 힘
    public string explosionEffect;    // 펑! 하는 이펙트 프리팹 (있다면)

    private bool hasExploded = false;

    void Start()
    {
        // 발사 방향으로 속도 부여
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
        if (photonView.IsMine) Invoke("DestroySelf", lifeTime);
    }

    void DestroySelf() {
        if (!hasExploded && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || hasExploded) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Map"))
        {
            PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                if (targetPV.OwnerActorNr == photonView.OwnerActorNr)
                {
                    return;
                }
            }
            Explode();

        }
    }
    void Explode()
    {
        hasExploded = true;
        CancelInvoke("DestroySelf");

        // 1. 시각적 이펙트 생성 (모든 클라이언트에게 보이도록 RPC나 포톤 생성 고려)
        if (explosionEffect != null)
        {
            PhotonNetwork.Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // 2. 주변 플레이어 체크 및 밀어내기
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Player"))
            {
                PhotonView targetPV = hit.gameObject.GetComponent<PhotonView>();

                if (targetPV != null)
                {
                    if (targetPV.OwnerActorNr == photonView.OwnerActorNr)
                    {
                        continue;
                    }

                    ApplyKnockback(hit.gameObject);
                }
            }
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    void ApplyKnockback(GameObject target)
    {
        // 폭발 중심에서 타겟까지의 방향 계산
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0.5f; // 약간 위로 붕 뜨게 만듦 (배그 수류탄 느낌)

        PhotonView targetPV = target.GetComponent<PhotonView>();
        if (targetPV != null)
        {
            // 맞는 사람의 Owner에게 RPC를 쏴서 "너 넉백 당해라"라고 알려줌
            targetPV.RPC("RPC_AddKnockback", targetPV.Owner, direction * explosionForce);
        }
    }
}