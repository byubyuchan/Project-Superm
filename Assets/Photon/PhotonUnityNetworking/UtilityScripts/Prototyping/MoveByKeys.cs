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
        public float rotationSpeed = 0.1f;

        private CharacterController controller;
        private Animator animator;
        private Vector3 velocity;           // 수직 속도 (중력/점프용)
        private bool isGrounded;

        [Header("Rotation Settings")]
        public Transform cameraPivot;
        public float mouseSensitivity = 3f;
        public float upRange = 70f;
        public float downRange = 20f;

        private float verticalRotation = 0f; // 현재 수직 회전값 저장용

        [Header("Projectile Settings")]
        public string projectilePrefabName = "MyArrow"; // Resources 폴더 내 프리팹 이름
        public Transform firePoint;

        private Vector3 impact = Vector3.zero;

        public bool isUIMode = false;
        public bool isMenuOpen = false;

        [Header("Aim Settings")]
        public LayerMask aimLayerMask;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            // 본인 소유가 아니면 스크립트 비활성화
            enabled = photonView.IsMine;

            if (photonView.IsMine)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

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

            if(!isMenuOpen && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)))
            {
                isUIMode = !isUIMode;
            }

            // 채팅 입력 중인지 체크 (UI 입력 필드가 선택된 경우 이동/공격 입력 무시)
            bool isChatting = false;

            if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                GameObject selectedUI = EventSystem.current.currentSelectedGameObject;

                if (EventSystem.current.currentSelectedGameObject.GetComponent("TMP_InputField") != null)
                {
                    isChatting = true;

                    if (selectedUI.name.ToLower().Contains("chat"))
                    {
                        isUIMode = false;
                    }
                }
            }

            if (isChatting && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                EventSystem.current.SetSelectedGameObject(null);
                isChatting = false;
            }

            if (isUIMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (!isChatting && !isUIMode)
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                transform.Rotate(Vector3.up * mouseX);

                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                verticalRotation -= mouseY;
                verticalRotation = Mathf.Clamp(verticalRotation, -downRange, upRange);

                if (cameraPivot != null)
                {
                    // 피벗의 로컬 X축 회전 적용
                    cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
                }
            }

            // 2. 입력 받기
            float horizontalInput = (isChatting || isUIMode) ? 0f : Input.GetAxisRaw("Horizontal");
            float verticalInput = (isChatting || isUIMode) ? 0f : Input.GetAxisRaw("Vertical");

            bool isAttacking = animator.GetCurrentAnimatorStateInfo(1).IsName("Attack");

            // 애니메이션 파라미터 전달 (보간 적용)
            if (animator != null)
            {
                animator.SetFloat("H", horizontalInput, 0.1f, Time.deltaTime);
                animator.SetFloat("V", verticalInput, 0.1f, Time.deltaTime);
                animator.SetBool("IsGround", isGrounded);
            }


            if (!isChatting && !isUIMode && Input.GetMouseButtonDown(0) && !isAttacking)
            {
                photonView.RPC("RPC_TriggerAction", RpcTarget.All, "Attack");
            }

            // 수평 이동 처리
            Vector3 moveDir = (transform.forward * verticalInput) + (transform.right * horizontalInput);

            if (impact.magnitude > 0.2f)
            {
                // 5는 감쇠 속도입니다. 더 빠르게 멈추게 하려면 이 숫자를 키우세요.
                impact = Vector3.Lerp(impact, Vector3.zero, 5f * Time.deltaTime);
            }
            else
            {
                impact = Vector3.zero;
            }

            Vector3 finalHorizontalMove = (moveDir.normalized * Speed) + impact;
            controller.Move(finalHorizontalMove * Time.deltaTime);

            // 점프 처리
            if (!isChatting && !isUIMode && Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                photonView.RPC("RPC_TriggerAction", RpcTarget.All, "Jump");
            }

            // 6. 중력 적용
            velocity.y += Gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

        }
        void Shoot()
        {
            if (!photonView.IsMine) return;

            // 1. 화면 중앙(조준선)에서 월드 공간으로 레이를 쏩니다.
            // 0.5, 0.5는 화면 정중앙을 의미합니다.
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            // 2. 레이캐스트 (본인 캐릭터 레이어는 무시해야 함)
            // 100m 거리 안에 부딪힌 게 있다면 그곳을 조준점으로, 없다면 100m 앞 허공을 조준점으로 잡습니다.
            if (Physics.Raycast(ray, out hit, 100f, ~aimLayerMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            // 3. 방향 계산: (목표 지점 - 왼손 총구 위치)
            // 캐릭터 정면이 아니라, 조준선이 가리키는 월드의 그 지점을 향하게 합니다.
            Vector3 aimDirection = (targetPoint - firePoint.position).normalized;

            // 4. 발사 (회전값은 aimDirection을 바라보게 설정)
            PhotonNetwork.Instantiate(projectilePrefabName, firePoint.position, Quaternion.LookRotation(aimDirection));
        }

        //void Shoot()
        //{
        //    if (!photonView.IsMine) return;

        //    if (string.IsNullOrEmpty(projectilePrefabName)) return;


        //    // 마우스 상하 회전값이 적용된 카메라 피벗의 방향을 참고하여 발사
        //    // 캐릭터 정면이 아니라 "카메라가 바라보는 곳"으로 날아가야 조준이 쉽습니다.
        //    Quaternion shootRotation = cameraPivot != null ? cameraPivot.rotation : transform.rotation;

        //    // 포톤 네트워크 상에 투사체 생성
        //    PhotonNetwork.Instantiate(projectilePrefabName, firePoint.position, shootRotation);
        //}

        [PunRPC]
        public void RPC_AddKnockback(Vector3 force)
        {
            if (photonView.IsMine)
            {
                impact += force;
                velocity.y = 0.5f;
            }
        }

        [PunRPC]
        public void RPC_TriggerAction(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}


// ====================================================
//if (Mathf.Abs(horizontalInput) > 0.1f)
//{
//    // 앞뒤 이동 방향에 따라 회전 방향 반전 처리 (기존 로직 유지)
//    float directionModifier = (verticalInput < -0.1f) ? -1f : 1f;
//    transform.Rotate(Vector3.up * horizontalInput * directionModifier * rotationSpeed * Time.deltaTime);
//}

//// 회전 처리
//if (Mathf.Abs(horizontalInput) > 0.1f)
//{
//    float directionModifier = (verticalInput < -0.1f) ? -1f : 1f;
//    transform.Rotate(Vector3.up * horizontalInput * directionModifier * rotationSpeed * Time.deltaTime);
//}
// ====================================================