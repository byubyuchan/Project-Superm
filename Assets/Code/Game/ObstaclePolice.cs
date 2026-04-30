using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ObstaclePolice : Obstacle
{
    [Header("Police AI Settings")]
    public float detectionRadius = 10f;
    public float abandonDistance = 20f;
    public LayerMask chaseLayer;

    private NavMeshAgent agent;
    private Vector3 originPosition;
    private Transform targetPlayer;
    private bool isChasing = false;

    private Collider[] hitResults = new Collider[20];

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originPosition = transform.position;
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
        // Physics.OverlapSphere 대신 NonAlloc 사용
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitResults, chaseLayer);

        if (count > 0)
        {
            targetPlayer = hitResults[0].transform;
            isChasing = true;
            agent.speed = 200f;
        }
    }

    void StopChasing()
    {
        isChasing = false;
        targetPlayer = null;
        agent.SetDestination(originPosition);
        agent.speed = 100f;
    }

    protected override void OnPlayerHit(GameObject player)
    {
        base.OnPlayerHit(player);
    }

    private void OnDrawGizmos()
    {
        // 1. 감지 범위 (Detection Radius) - 빨간
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 2. 추격 포기 범위 (Abandon Distance) - 빨간색
        // 추격 중일 때는 더 명확하게 보이도록 설정
        Gizmos.color = isChasing ? Color.red : new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, abandonDistance);

        // 4. 추격 대상 표시 (Target Player) - 노란색 선
        if (isChasing && targetPlayer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPlayer.position);
        }
    }
}