using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

// 플레이어의 공격 쿨다운을 크로스헤어 UI로 표시하는 스크립트
public class CrosshairCooldown : MonoBehaviour
{
    [Header("Dependencies")]
    public MoveByKeys playerMovement;
    public Image cooldownImage;

    // 플레이어가 풀링으로 재사용되기 때문에 이 스크립트 또한 재활성화 될 때마다 초기화 필요
    void OnEnable()
    {
        FindMyPlayer();
    }

    // IsMine으로 내 플레이어를 찾아 참조
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

        // 공격 쿨다운 진행 상황 계산
        float timePassed = Time.time - playerMovement.lastAttackTime;
        float progress = Mathf.Clamp01(timePassed / playerMovement.attackCooldown);
        cooldownImage.fillAmount = progress;

        cooldownImage.color = (progress < 1f) ? new Color(0, 0, 0, 0.5f) : Color.black;
    }
}