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
            //manager.TeleportCharacter(other.gameObject, fallbackPosition, fallbackRotation);
            manager.RequestTeleport(pv.gameObject);
        }
    }
}