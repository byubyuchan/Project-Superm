using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TPZone : MonoBehaviour
{
    [Header("Temp Fallback")]
    // Set empty if it is RunScene
    [SerializeField] private GameObject fallbackTarget;

    private Vector3 fallbackPosition;
    private Quaternion fallbackRotation;

    private void Start()
    {
        if (fallbackTarget != null)
        {
            fallbackPosition = fallbackTarget.transform.position;
            fallbackRotation = fallbackTarget.transform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;

        // FindFirstObjectByType은 싱글톤보다 유연하게 작동합니다.
        BaseGameManager manager = Object.FindFirstObjectByType<BaseGameManager>();

        if (manager != null)
        {
            // 매니저에게 최적의 부활 지점을 물어봄
            if (manager.GetBestRespawnPoint(out Vector3 resPos, out Quaternion resRot))
            {
                manager.TeleportCharacter(other.gameObject, resPos, resRot);
                return;
            }
        }
        // 리스폰 위치가 정해진 상태라면 그 위치로 리스폰
        if (fallbackTarget != null)
        {
            TeleportCharacterLocal(other.gameObject, fallbackPosition, fallbackRotation);
        }
    }
    private void TeleportCharacterLocal(GameObject playerObj, Vector3 pos, Quaternion rot)
    {
        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObj.transform.position = pos;
        playerObj.transform.rotation = rot;

        if (cc != null) cc.enabled = true;
    }
}