using UnityEngine;
using Photon.Pun;

public class EffectManager : MonoBehaviourPun
{
    public static EffectManager Instance;

    public GameObject[] explosionEffects;
    public GameObject[] localScreenEffects;

    void Awake() => Instance = this;

    // 1. 모두에게 보이는 폭발
    public void RequestExplosion(int index, Vector3 pos)
    {
        photonView.RPC("RPC_PlayEffect", RpcTarget.All, index, pos);
    }

    // 2. 나만 보이는 피격 효과
    public void RequestLocalEffect(int index, PhotonView targetPV)
    {
        if (targetPV == null || targetPV.Owner == null) return;

        photonView.RPC("RPC_PlayLocalEffect", targetPV.Owner, index);
    }

    [PunRPC]
    void RPC_PlayEffect(int index, Vector3 pos)
    {
        if (index < explosionEffects.Length)
        {
            GameObject fx = PhotonNetwork.Instantiate("VFX/" + explosionEffects[index].name, pos, Quaternion.identity);
            Destroy(fx, 2.0f);
        }
    }

    [PunRPC]
    void RPC_PlayLocalEffect(int index)
    {
        if (index < localScreenEffects.Length)
        {
            GameObject effectObj = localScreenEffects[index];

            effectObj.SetActive(true);

            if (effectObj.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                ps.Play(true);
            }
        }
    }
}