using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RunPlayerSlot : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText; // 점수나 진행도 표시
    public Image rankIcon; // 필요 시 순위 아이콘

    public Player TargetPlayer { get; private set; }

    public void Setup(Player player)
    {
        TargetPlayer = player;
        nameText.text = player.NickName;
        UpdateScore(0); // 초기 점수
    }
    public void SetRankIcon(bool active)
    {
        if (rankIcon != null)
        {
            rankIcon.gameObject.SetActive(active);
        }
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"{score} / 3 laps "; // 예: 달린 거리
    }
}