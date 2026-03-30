using Photon.Pun;
using UnityEngine.AI;
using UnityEngine;

[RequireComponent(typeof(NavMeshAgent), typeof(PhotonView))]
public class NPC : MonoBehaviourPun
{
    public float wanderRadius = 150f;
    public float wanderTimer = 7f;

    private NavMeshAgent agent;
    private float timer;

    [Header("Settings")]
    [SerializeField]
    private int explosionEffectIndex = 1;
    [SerializeField]
    private string[] respawnPrefabs;
    [SerializeField]
    private float respawnTime;
    public bool isExploded = false;

    void OnEnable()
    {
        isExploded = false;

        if (PhotonNetwork.IsMasterClient)
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            agent.enabled = true;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;

        if (!PhotonNetwork.IsMasterClient)
        {
            agent.enabled = false;
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        timer += Time.deltaTime;

        if (timer >= wanderTimer || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 newPos = RandomNavMeshLocation(wanderRadius);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public Vector3 RandomNavMeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = transform.position;

        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    void OnTriggerEnter(Collider other)
    {

        if (!PhotonNetwork.IsMasterClient || isExploded) return;

        // 장애물(Obstacle) 혹은 투사체(Projectile) 태그 확인
        if (other.CompareTag("Obstacle") || other.CompareTag("Projectile"))
        {

            isExploded = true;

            if (EffectManager.Instance != null)
            {
                EffectManager.Instance.RequestExplosion(explosionEffectIndex, transform.position);
            }

            if (RespawnManager.Instance != null)
            {
                int randomIndex = Random.Range(0, respawnPrefabs.Length);
                string selectedPrefab = respawnPrefabs[randomIndex];
                RespawnManager.Instance.RespawnNPC(selectedPrefab, transform.position, respawnTime);
            }
            Death();
        }
    }

    void Death()
    {
        PhotonNetwork.Instantiate("Item", transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(gameObject);
    }

}
