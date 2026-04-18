using Photon.Pun;
using Photon.Pun.UtilityScripts;
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

    [Header("Laser Settings")]
    [SerializeField] private LineRenderer laserLine;

    private Transform targetTransform;

    private new void OnEnable()
    {
        base.OnEnable();

        if (laserLine != null) laserLine.enabled = false;
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

            RotateTowards(targetTransform.position);

            DrawLaser(targetTransform.position);

            if (lockOnTimer >= lockOnTime)
            {
                FireRPC();
                lockOnTimer = 0;
            }
        }
        else
        {
            if (laserLine != null) laserLine.enabled = false;
            lockOnTimer = Mathf.Max(0, lockOnTimer - Time.deltaTime);
            base.Update();
        }
    }

    void DetectClosestPlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, targets, playerLayer);

        float closestDist = float.MaxValue;
        Transform bestTarget = null;

        for (int i = 0; i < count; i++)
        {
            Transform t = targets[i].transform;

            Transform tAimPoint = t.GetComponent<MoveByKeys>().aimPoint;

            Vector3 dirToTarget = (t.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)
            {
                float dist = Vector3.Distance(transform.position, t.position);

                if (dist < closestDist)
                {
                    if (IsTargetVisible(tAimPoint.position, t))
                    {
                        closestDist = dist;
                        bestTarget = t;
                    }
                }
            }
        }

        if (targetPlayer != bestTarget)
        {
            targetPlayer = bestTarget;
            lockOnTimer = 0;
        }

        if (targetPlayer != null)
        {
            targetTransform = targetPlayer.GetComponent<MoveByKeys>().aimPoint;
        }
    }

    bool IsTargetVisible(Vector3 targetPos, Transform targetRoot)
    {
        Vector3 start = firePoint.position;
        Vector3 dir = (targetPos - start).normalized;
        float dist = Vector3.Distance(start, targetPos);
        int layerMask = playerLayer | LayerMask.GetMask("Map");

        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit, dist, layerMask))
        {
            if (hit.transform.root == targetRoot.root)
            {
                return true;
            }
        }
        return false;
    }

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

        Vector3 fireDir = (targetTransform.position - firePoint.position).normalized;

        PhotonNetwork.Instantiate("NPC/"+projectileName, firePoint.position, Quaternion.LookRotation(fireDir));
    }

    void DrawLaser(Vector3 targetPos)
    {
        if (laserLine == null) return;

        if (!laserLine.enabled) laserLine.enabled = true;

        float currentWidth = Mathf.Lerp(0.3f, 0.01f, lockOnTimer / lockOnTime);

        laserLine.startWidth = currentWidth;

        laserLine.SetPosition(0, firePoint.position);

        Vector3 dir = (targetPos - firePoint.position).normalized;
        RaycastHit hit;

        int layerMask = playerLayer | LayerMask.GetMask("Map");

        if (Physics.Raycast(firePoint.position, dir, out hit, detectionRange, layerMask))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, firePoint.position + (dir * detectionRange));
        }
    }
}