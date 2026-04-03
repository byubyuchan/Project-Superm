using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TPZone : MonoBehaviour
{
    [Header("Temp Fallback For Warmup Scene")]
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

        if (pv != null && pv.IsMine)
        {
            if (RunGameManager.Instance != null)
            {
                Player targetPlayer = PhotonNetwork.LocalPlayer;

                // Check if the player has a last checkpoint position saved
                if (targetPlayer.CustomProperties.ContainsKey(RunGameManager.PROP_LAST_X))
                {
                    float x = (float)targetPlayer.CustomProperties[RunGameManager.PROP_LAST_X];
                    float y = (float)targetPlayer.CustomProperties[RunGameManager.PROP_LAST_Y];
                    float z = (float)targetPlayer.CustomProperties[RunGameManager.PROP_LAST_Z];
                    float rotY = (float)targetPlayer.CustomProperties[RunGameManager.PROP_LAST_ROT_Y];

                    Vector3 respawnPos = new Vector3(x, y, z);
                    Quaternion respawnRot = Quaternion.Euler(0, rotY, 0);

                    if (RunGameManager.Instance != null)
                    {
                        RunGameManager.Instance.TeleportCharacter(other.gameObject, respawnPos, respawnRot);
                    }
                }

                // If no last checkpoint, check for initial position
                else if (targetPlayer.CustomProperties.ContainsKey(RunGameManager.PROP_INIT_X))
                {
                    float initX = (float)targetPlayer.CustomProperties[RunGameManager.PROP_INIT_X];
                    float initY = (float)targetPlayer.CustomProperties[RunGameManager.PROP_INIT_Y];
                    float initZ = (float)targetPlayer.CustomProperties[RunGameManager.PROP_INIT_Z];
                    float initRotY = (float)targetPlayer.CustomProperties[RunGameManager.PROP_INIT_ROT_Y];

                    Vector3 initPos = new Vector3(initX, initY, initZ);
                    Quaternion initRot = Quaternion.Euler(0, initRotY, 0);

                    if (RunGameManager.Instance != null)
                    {
                        RunGameManager.Instance.TeleportCharacter(other.gameObject, initPos, initRot);
                    }
                    return;
                }
            }
            if (fallbackTarget != null)
            {
                TeleportCharacterLocal(other.gameObject, fallbackPosition, fallbackRotation);
            }
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