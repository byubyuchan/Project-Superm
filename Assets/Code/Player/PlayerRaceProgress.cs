using UnityEngine;
using Photon.Pun;

public class PlayerRaceProgress : MonoBehaviourPun
{
    private void Start()
    {
        // Report the initial position
        if (photonView.IsMine && RunGameManager.Instance != null)
        {
            RunGameManager.Instance.ReportAndInitializePlayerInitialPos(this.gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // Check if the trigger is a checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            if (RunGameManager.Instance != null)
            {
                RunGameManager.Instance.ProcessLocalPlayerCheckpointTrigger(this.gameObject, other.transform);
            }
        }
    }
}
