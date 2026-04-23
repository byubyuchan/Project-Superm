using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairCooldown : MonoBehaviour
{
    [Header("Dependencies")]
    public MoveByKeys playerMovement;
    public Image cooldownImage;

    void OnEnable()
    {
        FindMyPlayer();
    }
    private void FindMyPlayer()
    {
        var allPlayers = FindObjectsByType<MoveByKeys>(FindObjectsSortMode.None);
        foreach (var p in allPlayers)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                playerMovement = p;
                break;
            }
        }
    }

    void Update()
    {
        // 만약 플레이어를 잃어버렸다면(캐릭터 파괴/재생성 시) 다시 찾기 시도
        if (playerMovement == null)
        {
            FindMyPlayer();
            if (playerMovement == null) return;
        }

        if (cooldownImage == null) return;

        float timePassed = Time.time - playerMovement.lastAttackTime;
        float progress = Mathf.Clamp01(timePassed / playerMovement.attackCooldown);
        cooldownImage.fillAmount = progress;

        cooldownImage.color = (progress < 1f) ? new Color(0, 0, 0, 0.5f) : Color.black;
    }
}