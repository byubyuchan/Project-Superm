using UnityEngine;
using Photon.Pun;

public class TPZone : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private Vector3 targetPosition;

    private void Start()
    {
        if (target != null)
        {
            targetPosition = target.transform.position;
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
                CharacterController cc = other.GetComponent<CharacterController>();

                if (cc != null)
                {
                    cc.enabled = false;

                    other.transform.position = targetPosition;

                    cc.enabled = true;
                }
            }
        }
    }
}