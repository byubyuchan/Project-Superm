using Photon.Pun;
using UnityEngine;

public class TrapPlatformFall : MovingPlatform
{
    public float fallSpeed = 20f;      // 낙하 속도
    public float restoreSpeed = 2f;    // 복귀 속도 (다시 올라올 때)
    public float delayTime = 0.5f;     // 밟고 나서 떨어지기까지 대기 시간
    public float resetTime = 3f;       // 떨어진 후 다시 제자리로 돌아오기까지 시간
    public float fallDistance = 20f;   // 얼마나 깊이 떨어질지

    private Vector3 startPosition;
    private Vector3 lastPosition;

    private float timer = 0f;
    private bool isActivated = false;

    void Start() { startPosition = lastPosition = transform.position; }

    protected override void HandlePlatformMovement()
    {
        // 마스터 클라이언트가 발판의 상태를 결정합니다.
        if (playersOnPlatform.Count > 0 && !isActivated)
        {
            isActivated = true;
            timer = 0f; // 타이머 시작
        }

        if (isActivated)
        {
            timer += Time.fixedDeltaTime;

            if (timer < delayTime)
            {
                return;
            }
            else if (timer < delayTime + 1f) // 1초 동안 급속 낙하
            {
                Vector3 targetPos = startPosition + (Vector3.down * fallDistance);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.fixedDeltaTime);
            }
            else if (timer > resetTime) // 리셋 시간이 지나면 복구
            {
                if (playersOnPlatform.Count > 0)
                {
                    timer = delayTime;
                    return;
                }
                transform.position = Vector3.MoveTowards(transform.position, startPosition, restoreSpeed * Time.fixedDeltaTime);

                // 완전히 돌아오면 상태 초기화
                if (transform.position == startPosition)
                {
                    isActivated = false;
                }
            }
        }
    }

    protected override void UpdatePlayerPositions()
    {
        return;
    }
}