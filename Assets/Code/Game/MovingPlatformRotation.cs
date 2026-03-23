using Photon.Pun;
using UnityEngine;

public class MovingPlatformRotation : MovingPlatform
{
    public float rotationSpeed = 90f;

    protected override void HandlePlatformMovement()
    {
        transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
    }

    protected override void UpdatePlayerPositions()
    {
        float angle = rotationSpeed * Time.fixedDeltaTime;
        foreach (var player in playersOnPlatform)
        {
            if (player != null && player.enabled)
            {
                var pv = player.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Vector3 offset = player.transform.position - transform.position;
                    Vector3 rotatedOffset = Quaternion.Euler(0, angle, 0) * offset;
                    Vector3 moveDir = rotatedOffset - offset;
                    if (player.isGrounded) moveDir.y = -2f;
                    player.Move(moveDir);
                }
            }
        }
    }
}