using Photon.Pun;
using UnityEngine;

public class NPC_Sniper : NPC
{
    [Header("Sniper Settings")]
    public float viewAngle = 60f;
    public float lockOnTime = 3f;
    public float detectionRange = 50f;

    [Header("Current Status")]
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private float lockOnTimer = 0f;

    [Header("Optimization")]
    public LayerMask playerLayer; 
    private Collider[] targets = new Collider[20];

    [Header("Shoot Settings")]
    [SerializeField] 
    private Transform firePoint;
    [SerializeField]
    private string projectileName = "NPCProjectile";

    private new void OnEnable()
    {
        base.OnEnable();

        targetPlayer = null;
        lockOnTimer = 0f;
    }

    protected override void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        DetectClosestPlayer();

        if (targetPlayer != null)
        {
            lockOnTimer += Time.deltaTime;

            if (agent.hasPath) agent.ResetPath();

            RotateTowards(targetPlayer.position);

            if (lockOnTimer >= lockOnTime)
            {
                FireRPC();
                lockOnTimer = 0;
            }
        }
        else
        {
            lockOnTimer = Mathf.Max(0, lockOnTimer - Time.deltaTime);
            base.Update();
        }
    }

    void DetectClosestPlayer()
    {
        // NonAlloc으로 메모리 할당 없이 주변 플레이어만 훑음
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, targets, playerLayer);

        float closestDist = float.MaxValue;
        Transform bestTarget = null;

        for (int i = 0; i < count; i++)
        {
            Transform t = targets[i].transform;

            // 거리 및 각도 체크
            Vector3 dir = (t.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) < viewAngle * 0.5f)
            {
                float dist = Vector3.Distance(transform.position, t.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTarget = t;
                }
            }
        }

        // 타겟 변경 시에만 타이머 초기화 (안정적인 조준 유지)
        if (targetPlayer != bestTarget)
        {
            targetPlayer = bestTarget;
            lockOnTimer = 0;
        }
    }

    // IsPlayerInSight나 FindClosestVisiblePlayer는 이제 필요 없으므로 삭제해도 됩니다.

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void FireRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (targetPlayer == null) return;

        Vector3 fireDir = (targetPlayer.position - firePoint.position).normalized;

        PhotonNetwork.Instantiate("NPC/"+projectileName, firePoint.position, Quaternion.LookRotation(fireDir));
    }
}