using UnityEngine;
using Photon.Pun;

public class TPZone : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    [SerializeField] private bool isLastZone = false;

    private void Start()
    {
        if (target != null)
        {
            targetPosition = target.transform.position;
            targetRotation = target.transform.rotation;
        }
        else
        {
            Debug.LogError("TPZone: Target is not assigned.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pv != null)
        {
            if (pv.IsMine)
            {
                if (isLastZone)
                {
                    RunGameManager manager = Object.FindFirstObjectByType<RunGameManager>();

                    if (manager != null)
                    {
                        manager.OnPlayerReachedFinish(other.gameObject);
                    }
                }
                CharacterController cc = other.GetComponent<CharacterController>();

                if (cc != null)
                {
                    cc.enabled = false;

                    other.transform.position = targetPosition;
                    other.transform.rotation = targetRotation;

                    cc.enabled = true;
                }
            }
        }
    }
}