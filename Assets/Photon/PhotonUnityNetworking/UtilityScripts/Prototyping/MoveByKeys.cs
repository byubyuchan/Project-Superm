using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

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

            // 채팅 입력 중인지 체크 (UI 입력 필드가 선택된 경우 이동/공격 입력 무시)
            bool isChatting = false;
            if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                if (EventSystem.current.currentSelectedGameObject.GetComponent("TMP_InputField") != null)
                {
                    isChatting = true;
                }
            }

            // 2. 입력 받기
            float horizontalInput = isChatting ? 0f : Input.GetAxisRaw("Horizontal");
            float verticalInput = isChatting ? 0f : Input.GetAxisRaw("Vertical");

            bool isAttacking = animator.GetCurrentAnimatorStateInfo(1).IsName("Attack");

            // 애니메이션 파라미터 전달 (보간 적용)
            if (animator != null)
            {
                animator.SetFloat("H", horizontalInput, 0.1f, Time.deltaTime);
                animator.SetFloat("V", verticalInput, 0.1f, Time.deltaTime);
                animator.SetBool("IsGround", isGrounded);
            }


            if (!isChatting && Input.GetMouseButtonDown(0) && !isAttacking)
            {
                animator.SetTrigger("Attack");
            }

            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // 앞뒤 이동 방향에 따라 회전 방향 반전 처리 (기존 로직 유지)
                float directionModifier = (verticalInput < -0.1f) ? -1f : 1f;
                transform.Rotate(Vector3.up * horizontalInput * directionModifier * rotationSpeed * Time.deltaTime);
            }

            // 회전 처리
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                float directionModifier = (verticalInput < -0.1f) ? -1f : 1f;
                transform.Rotate(Vector3.up * horizontalInput * directionModifier * rotationSpeed * Time.deltaTime);
            }

            // 수평 이동 처리
            Vector3 move = transform.forward * verticalInput * Speed;
            controller.Move(move * Time.deltaTime);

            // 점프 처리
            if (!isChatting && Input.GetButtonDown("Jump") && isGrounded)
            {
                animator.SetTrigger("Jump");
                velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // 6. 중력 적용
            velocity.y += Gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}