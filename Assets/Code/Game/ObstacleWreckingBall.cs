using UnityEngine;
using Photon.Pun;

public class ObstacleWreckingBall : Obstacle
{
    [Header("Pendulum Settings")]
    public Transform pivot;
    public float swingSpeed = 2f;
    public float swingAngle = 45f;
    public float offset = 0f;

    private void Update()
    {
        if (pivot != null)
        {
            double time = PhotonNetwork.Time;
            float angle = (float)(System.Math.Sin(time * swingSpeed + offset) * swingAngle);
            pivot.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    protected override void OnPlayerHit(GameObject player)
    {
        PhotonView targetPV = player.GetComponent<PhotonView>();

        if (targetPV != null)
        {
            Debug.Log($"레킹볼이 [{player.name}]을(를) 쳤습니다");

            Vector3 pushDir = (player.transform.position - transform.position).normalized;
            pushDir.y = 0f; // 수평으로만 날아가게
            pushDir = pushDir.normalized;

            // 플레이어에게 넉백 명령 전송
            targetPV.RPC("RPC_AddKnockback", RpcTarget.All, pushDir * knockbackForce);
        }
        else
        {
            Debug.LogWarning("맞은 플레이어에게 PhotonView가 없습니다");
        }
    }
}
