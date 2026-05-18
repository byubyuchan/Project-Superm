using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public static PlayerSpawner Instance;

    [Header("Player Prefabs (Resources 폴더 내의 이름과 일치해야 함)")]
    [SerializeField] private GameObject[] playerPrefabs;

    protected GameObject player;

    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private Transform[] spawnZones;

    [Header("Camera Settings")]
    [SerializeField]
    private GameObject ghostCamera;
    private GameObject currentCamera;

    [Header("Dead Effect Settings")]
    [SerializeField] private int effectIndex;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        float timeout = 5f;
        while (!PhotonNetwork.InRoom && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
            if (canvas != null) canvas.SetActive(false);
        }
        else
        {
            Debug.LogError("방 입장 대기 시간 초과! 네트워크 연결을 확인하세요.");
        }
    }
    public void RequestRespawn(GameObject player, float delay)
    {
        StartCoroutine(RespawnRoutine(player, delay));
    }

    private IEnumerator RespawnRoutine(GameObject player, float delay)
    {
        if (ghostCamera!= null && currentCamera == null)
        {
            currentCamera = Instantiate(ghostCamera, player.transform.position + new Vector3(0,15f,0), player.transform.rotation);
        }

        EffectManager.Instance.RequestExplosion(effectIndex,player.transform.position);

        PhotonNetwork.Destroy(player);

        yield return new WaitForSeconds(delay);

        if (currentCamera != null)
        {
            Destroy(currentCamera);
        }

        ReSpawn();
    }

    public void SpawnPlayer()
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0) return;

        if (PhotonNetwork.InRoom)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    PhotonNetwork.Destroy(p);
                    Spawn();
                    return;
                }
            }
            Spawn();
        }
    }

    public void Spawn()
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0) return;

        int randomPrefabIndex = Random.Range(0, playerPrefabs.Length);
        GameObject selectedPrefab = playerPrefabs[randomPrefabIndex];

        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        if (spawnZones.Length > 0)
        {
            int index = Random.Range(0, spawnZones.Length);
            spawnPos = spawnZones[index].position;
            spawnRot = spawnZones[index].rotation;
        }

        player = PhotonNetwork.Instantiate(selectedPrefab.name, spawnPos, spawnRot);
    }

    public void ReSpawn()
    {
        int randomPrefabIndex = Random.Range(0, playerPrefabs.Length);
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        // 체크포인트가 있는지 확인
        BaseGameManager gameManager = Object.FindFirstObjectByType<BaseGameManager>();
        if (gameManager != null && gameManager.GetBestRespawnPoint(out Vector3 targetPos, out Quaternion targetRot))
        {
            spawnPos = targetPos;
            spawnRot = targetRot;
        }
        // 부활장소가 랜덤인 모드인지 확인
        else if(spawnZones.Length > 0)
        {
            int index = Random.Range(0, spawnZones.Length);
            spawnPos = spawnZones[index].position;
            spawnRot = spawnZones[index].rotation;
        }
        // 아무것도 아니라면 첫 부활장소에서 리스폰
        player = PhotonNetwork.Instantiate(playerPrefabs[randomPrefabIndex].name, spawnPos, spawnRot);
    }
    #region PunCallbacks 옵션
    public override void OnLeftRoom() { Debug.Log("방을 떠났습니다."); }
    public override void OnCreateRoomFailed(short returnCode, string message) { Debug.LogError($"방 생성 실패: {message}"); }
    public override void OnJoinRoomFailed(short returnCode, string message) { Debug.LogError($"방 참가 실패: {message}"); }
    #endregion
}