using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Projectile : MonoBehaviourPun
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 10f;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;   // 폭발 반경
    public float explosionForce = 15f;    // 밀어내는 힘

    [SerializeField]
    protected int explosionEffectIndex = 0;

    protected bool hasExploded = false;

    protected bool isNPCProjectile;

    public void OnEnable()
    {
        hasExploded = false;

        isNPCProjectile = gameObject.CompareTag("NPCProjectile");

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = transform.forward * speed;
        }

        if (photonView.IsMine)
        {
            CancelInvoke("DestroySelf");
            Invoke("DestroySelf", lifeTime);
        }
    }

    void DestroySelf() {

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // 투사체는 트리거를 끄고 충돌 처리
    virtual protected void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || hasExploded) return;

        // Map이나 Player 태그 확인
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Map"))
        {
            // 1. Player일 경우 팀킬 방지 로직
            if (collision.gameObject.CompareTag("Player") && !isNPCProjectile)
            {
                PhotonView targetPV = collision.gameObject.GetComponent<PhotonView>();
                if (targetPV != null && targetPV.OwnerActorNr == photonView.OwnerActorNr)
                {
                    return;
                }
            }

            Explode();
        }
    }

    // 땅울림 같은 광역기는 이펙트가 따로 없고, 트리거가 켜져있어야 함.
    virtual protected void OnTriggerEnter(Collider col)
    {
        if (!photonView.IsMine || hasExploded) return;

        // 3. Player에 닿으면 넉백 처리
        if (col.gameObject.CompareTag("Player"))
        {
            PhotonView targetPV = col.gameObject.GetComponent<PhotonView>();

            if (targetPV != null)
            {
                // 팀킬 방지: 자신(발사자)은 제외
                if (targetPV.OwnerActorNr == photonView.OwnerActorNr) return;

                if (damage > 0f)
                {
                    targetPV.RPC("RPC_TakeDamage", targetPV.Owner, damage);
                }

                ApplyKnockback(col.gameObject);

                hasExploded = true;
            }
        }
    }

    virtual protected void Explode(GameObject target = null)
    {
        hasExploded = true;
        CancelInvoke("DestroySelf");

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.RequestExplosion(explosionEffectIndex, transform.position);
        }

        // 주변 플레이어 체크 및 밀어내기
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Player"))
            {
                PhotonView targetPV = hit.gameObject.GetComponent<PhotonView>();

                if (targetPV != null)
                {
                    if (targetPV.OwnerActorNr == photonView.OwnerActorNr && !isNPCProjectile)
                    {
                        continue;
                    }

                    ApplyKnockback(hit.gameObject);

                    if (damage > 0f)
                    {
                        targetPV.RPC("RPC_TakeDamage", targetPV.Owner, damage);
                    }
                }
            }
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void ExplodeNoFX()
    {
        hasExploded = true;

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
    private void OnDrawGizmos()
    {
        // 폭발 중심점 시각화 (빨간색 구체)
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // 투명도 30% 빨간색
        Gizmos.DrawSphere(transform.position, explosionRadius);

        // 테두리 선
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}