using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairCooldown : MonoBehaviour
{
    [Header("Dependencies")]
    public MoveByKeys playerMovement;
    public Image cooldownImage;

    void Start()
    {
        playerMovement = FindAnyObjectByType<MoveByKeys>();

        // 처음에 꽉 차 있는 상태로 시작
        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f;
    }

    void Update()
    {
        if (playerMovement == null) 
        {
            var allPlayers = FindObjectsByType<MoveByKeys>(FindObjectsSortMode.None);
            foreach (var p in allPlayers)
            {
                if (p.GetComponent<PhotonView>().IsMine)
                {
                    playerMovement = p;
                    break;
                }
            }
        }

        if (playerMovement == null || cooldownImage == null) return;

        float timePassed = Time.time - playerMovement.lastAttackTime;
        float progress = Mathf.Clamp01(timePassed / playerMovement.attackCooldown);

        cooldownImage.fillAmount = progress;

        if (progress < 1f)
        {
            cooldownImage.color = new Color(1, 0, 0, 0.5f);
        }
        else
        {
            cooldownImage.color = Color.red;
        }
    }
}