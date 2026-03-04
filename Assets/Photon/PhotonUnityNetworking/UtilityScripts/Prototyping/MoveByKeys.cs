using Photon.Pun;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    [RequireComponent(typeof(CharacterController), typeof(PhotonView))]
    public class MoveByKeys : MonoBehaviourPun
    {
        public float Speed = 5f;            // 이동 속도 (기존 1000은 너무 컸으니 조정)
        public float JumpHeight = 2f;       // 점프 높이
        public float Gravity = -20f;        // 중력 세기
        public float rotationSpeed = 360f;

        private CharacterController controller;
        private Animator animator;
        private Vector3 velocity;           // 수직 속도 (중력/점프용)
        private bool isGrounded;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            // 본인 소유가 아니면 스크립트 비활성화
            enabled = photonView.IsMine;

            // CharacterController 사용 시 Rigidbody는 삭제하거나 IsKinematic을 켜야 합니다.
            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }
        }

        public void Update()
        {
            if (!photonView.IsMine) return;

            // 1. 바닥 체크
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                
                velocity.y = -2f; // 바닥에 붙어있도록 살짝 아래로 힘을 줌
            }

            // 2. 입력 받기
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            // 애니메이션 파라미터 전달 (보간 적용)
            if (animator != null)
            {
                animator.SetFloat("H", horizontalInput, 0.1f, Time.deltaTime);
                animator.SetFloat("V", verticalInput, 0.1f, Time.deltaTime);
            }

            // 3. 회전 처리
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // 앞뒤 이동 방향에 따라 회전 방향 반전 처리 (기존 로직 유지)
                float directionModifier = (verticalInput < -0.1f) ? -1f : 1f;
                transform.Rotate(Vector3.up * horizontalInput * directionModifier * rotationSpeed * Time.deltaTime);
            }

            // 4. 이동 처리 (Forward 방향)
            Vector3 move = transform.forward * verticalInput * Speed;
            controller.Move(move * Time.deltaTime);

            // 5. 점프 처리
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                // 물리 공식: v = sqrt(h * -2 * g)
                animator.SetTrigger("Jump");
                velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            animator.SetBool("IsGround", isGrounded);

            // 6. 중력 적용
            velocity.y += Gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}