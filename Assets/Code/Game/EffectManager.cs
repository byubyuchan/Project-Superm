using UnityEngine;
using Photon.Pun;

public class EffectManager : MonoBehaviourPun
{
    // 외부에서 쉽게 접근할 수 있도록 싱글톤 구성
    public static EffectManager Instance;

    public GameObject[] explosionEffects;

    void Awake()
    {
        Instance = this;
    }

    // 이펙트 재생 요청을 모든 클라이언트에게 전달하는 함수
    public void RequestExplosion(int index, Vector3 pos)
    {
        photonView.RPC("RPC_PlayEffect", RpcTarget.All, index, pos);
    }

    [PunRPC]
    void RPC_PlayEffect(int index, Vector3 pos)
    {
        if (index < explosionEffects.Length)
        {
            GameObject fx = Instantiate(explosionEffects[index], pos, Quaternion.identity);
            Destroy(fx, 2.0f);
        }
    }
}