using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RunPlayerSlot : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText; // 점수나 진행도 표시
    public TextMeshProUGUI rankText;

    public Player TargetPlayer { get; private set; }

    public void Setup(Player player)
    {
        TargetPlayer = player;
        nameText.text = player.NickName;
        UpdateScore(0); // 초기 점수
        UpdateRank(0);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"{score} / 3 Laps "; // 예: 달린 거리
    }

    public void UpdateRank(int rank)
    {
        if (rank <= 0)
        {
            rankText.text = "-";
            return;
        }

        string suffix = "th";
        int lastDigit = rank % 10;

        if (lastDigit == 1)
        {
            suffix = "st";
        }
        else if (lastDigit == 2)
        {
            suffix = "nd";
        }
        else if (lastDigit == 3)
        {
            suffix = "rd";
        }

        rankText.text = $"{rank}{suffix}";
    }
}