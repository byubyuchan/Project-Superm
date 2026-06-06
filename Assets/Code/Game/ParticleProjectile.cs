using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections.Generic;
using UnityEngine;

public class ParticleProjectile : Projectile
{
    [Header("Cooldown Settings")]
    public float particleEffectInterval = 0.25f;

    [Header("Particle Settings")]
    public float particleDamageInterval = 0.1f;
    private Dictionary<int, float> particleDamageTimes = new Dictionary<int, float>();

    private float lastEffectTime = 0f;

    protected override void OnEnable()
    {
        particleDamageTimes.Clear();
        base.OnEnable();
    }

    protected void ApplyEffect(GameObject other)
    {
        if (Time.time - lastEffectTime >= particleEffectInterval)
        {
            if (EffectManager.Instance != null)
            {
                if (other.CompareTag("Dummy") || other.CompareTag("Player"))
                {
                    PhotonView targetPV = other.GetPhotonView();

                    if (targetPV != null)
                    {
                        EffectManager.Instance.RequestAttachedExplosion(explosionEffectIndex, targetPV.ViewID);

                        EffectManager.Instance.RequestLocalEffect(sreenEffectIndex, targetPV);
                    }
                }
            }
            lastEffectTime = Time.time;
        }
    }

    virtual protected void OnParticleCollision(GameObject other)
    {
        PhotonView activePV = (photonView != null && photonView.ViewID != 0) ? photonView : GetComponentInParent<PhotonView>();

        if (activePV == null || !activePV.IsMine) return;

        if (other.CompareTag("Dummy"))
        {
            Debug.Log("<color=orange> Dummy 명중! (이펙트는 쿨타임 적용됨)</color>");
            ApplyEffect(other);
            return;
        }

        if (other.CompareTag("Player"))
        {
            PhotonView targetPV = other.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                // 팀킬 방지
                if (targetPV.OwnerActorNr == activePV.OwnerActorNr && !isNPCProjectile) return;

                int targetActorNr = targetPV.OwnerActorNr;

                // 데미지 쿨타임 체크
                if (particleDamageTimes.TryGetValue(targetActorNr, out float lastDmgTime))
                {
                    if (Time.time - lastDmgTime < particleDamageInterval) return;
                }

                // 데미지 타격 시간 갱신
                particleDamageTimes[targetActorNr] = Time.time;

                if (damage > 0f)
                {
                    targetPV.RPC("RPC_TakeDamage", targetPV.Owner, damage);
                }
                ApplyEffect(other);
                ApplyKnockback(other, targetPV);
            }
        }
    }
}