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

    public bool isFake = false;

    [SerializeField]
    private TrapPlatformFall sibling;

    void Start() 
    { 
        startPosition = lastPosition = transform.position;

        if (PhotonNetwork.IsMasterClient)
        {
            if (sibling != null)
            {
                isFake = Random.value > 0.5f;

                sibling.isFake = !this.isFake;

                // 모든 클라이언트에게 상태 동기화 (하지 않을 경우 호스트를 제외한 인원에게는 bool값 지정이 보이지 않음. 다만, 작동은 함.)
                // 작동 한다면서 RPC는 왜 쏘죠? 재미나이의 대답으론 타이밍이 늦을 수 있다고 함. Host의 bool값을 다른 클라이언트가 받는 타이밍이 달라서 그런듯.
                photonView.RPC("RPC_SetFakeStatus", RpcTarget.AllBuffered, isFake);
                sibling.photonView.RPC("RPC_SetFakeStatus", RpcTarget.AllBuffered, !isFake);
            }
        }
    }

    protected override void HandlePlatformMovement()
    {
        if (!isFake) return;

        if (playersOnPlatform.Count > 0 && !isActivated)
        {
            isActivated = true;
            timer = 0f;
        }

        if (isActivated)
        {
            timer += Time.fixedDeltaTime;

            if (timer < delayTime)
            {
                return;
            }
            else if (timer < delayTime + 3f) // 1초 동안 급속 낙하
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

    [PunRPC]
    public void RPC_SetFakeStatus(bool value)
    {
        this.isFake = value;
    }
}