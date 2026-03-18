using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ObstaclePolice : Obstacle
{
    [Header("Police AI Settings")]
    public float detectionRadius = 10f; // 플레이어 감지 거리
    public float abandonDistance = 20f;
    public LayerMask chaseLayer;       // 쫓아갈 레이어 (Player 레이어 설정)

    private NavMeshAgent agent;
    private Vector3 originPosition;
    private Transform targetPlayer;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originPosition = transform.position;
    }

    void OnDrawGizmosSelected()
    {
        // 감지 범위는 파란색
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 포기 범위는 빨간색 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, abandonDistance);
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isChasing && targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer > abandonDistance)
            {
                StopChasing();
            }
            else
            {
                // 플레이어 위치로 계속 이동
                agent.SetDestination(targetPlayer.position);
            }
        }
        else
        {
            DetectPlayer();
        }
    }

    void DetectPlayer()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, chaseLayer);
        if (targets.Length > 0)
        {
            targetPlayer = targets[0].transform;
            isChasing = true;
            agent.speed = 200f; // 추적 시 속도 증가
        }
    }

    void StopChasing()
    {
        isChasing = false;
        targetPlayer = null;
        agent.SetDestination(originPosition); // 원래 자리로 복귀
        agent.speed = 100f; // 복귀 시 속도 정상화
    }

    //bool IsInChaseLayer(GameObject obj)
    //{
    //    return (chaseLayer.value & (1 << obj.layer)) != 0;
    //}

    // 플레이어와 부딪히면 (부모 클래스 Obstacle의 로직 활용)
    protected override void OnPlayerHit(GameObject player)
    {
        base.OnPlayerHit(player); // 넉백 실행
    }
}