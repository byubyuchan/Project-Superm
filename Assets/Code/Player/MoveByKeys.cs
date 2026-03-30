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
        public float zoomUpRange = 90f;
        public float zoomDownRange = 40f;

        private float verticalRotation = 0f; // 현재 수직 회전값 저장용

        [Header("Projectile Settings")]
        public string projectile;
        public Transform firePoint;

        private Vector3 impact = Vector3.zero;

        public bool isUIMode = false;
        public bool isMenuOpen = false;

        public bool isLoadingAttack = false;

        [Header("Aim Settings")]
        public LayerMask aimLayerMask;

        [Header("Attack Settings")]
        public float attackCooldown = 0.5f;
        public float lastAttackTime;

        private float originalSpeed;
        float horizontalInput;
        float verticalInput;

        private Vector3 localSize;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            localSize = transform.localScale;

            // 본인 소유가 아니면 스크립트 비활성화
            enabled = photonView.IsMine;

            if (photonView.IsMine)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                originalSpeed = Speed;
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

                velocity.y = -100f; // 바닥에 붙어있도록 살짝 아래로 힘을 줌
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
                float currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);

                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * currentSensitivity;
                transform.Rotate(Vector3.up * mouseX);

                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * currentSensitivity;

                verticalRotation -= mouseY;
                if (isLoadingAttack) verticalRotation = Mathf.Clamp(verticalRotation, -zoomDownRange, zoomUpRange);
                else verticalRotation = Mathf.Clamp(verticalRotation, -downRange, upRange);

                if (cameraPivot != null)
                {
                    // 피벗의 로컬 X축 회전 적용
                    cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
                }
            }

            // 2. 입력 받기
            horizontalInput = (isChatting || isUIMode) ? 0f : Input.GetAxisRaw("Horizontal");
            verticalInput = (isChatting || isUIMode) ? 0f : Input.GetAxisRaw("Vertical");

            bool isAttacking = animator.GetCurrentAnimatorStateInfo(1).IsName("Attack");
            bool isReady = animator.GetCurrentAnimatorStateInfo(1).IsName("Ready");

            // 애니메이션 파라미터 전달 (보간 적용)
            if (animator != null)
            {
                animator.SetFloat("H", horizontalInput, 0.1f, Time.deltaTime);
                animator.SetFloat("V", verticalInput, 0.1f, Time.deltaTime);
                animator.SetBool("IsGround", isGrounded);
            }

            if (!isChatting && !isUIMode && Input.GetMouseButtonDown(1) && !isAttacking)
            {
                isLoadingAttack = !isLoadingAttack;
                photonView.RPC("RPC_LoadAction", RpcTarget.All, "ReadyToAttack", isLoadingAttack);
            }

            if (!isChatting && !isUIMode && Input.GetMouseButtonDown(0) && !isAttacking && isLoadingAttack) 
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    lastAttackTime = Time.time;
                    photonView.RPC("RPC_TriggerAction", RpcTarget.All, "Attack");
                }
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

            //isLoadingAttack = false;
            //photonView.RPC("RPC_LoadAction", RpcTarget.All, "ReadyToAttack", isLoadingAttack);

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 100f, ~aimLayerMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            Vector3 aimDirection = (targetPoint - firePoint.position).normalized;

            PhotonNetwork.Instantiate(projectile, firePoint.position, Quaternion.LookRotation(aimDirection));
        }

        void Quake()
        {
            if (!photonView.IsMine) return;
            if (isGrounded) PhotonNetwork.Instantiate(projectile, transform.position + new Vector3(2f, 4f, 0), Quaternion.identity);
        }

        public void ApplySpeedBoost(float additionalSpeed)
        {
            if (verticalInput > 0.1f) Speed = originalSpeed + additionalSpeed;
            else ResetSpeed();
        }

        public void ResetSpeed()
        {
            Speed = originalSpeed;
        }

        [PunRPC]
        public void RPC_AddKnockback(Vector3 force)
        {
            if (photonView.IsMine)
            {
                impact += force;
                velocity.y = 0.5f;
            }
        }

        [PunRPC] // 좌클릭 공격
        public void RPC_TriggerAction(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }

        [PunRPC] // 우클릭 줌
        public void RPC_LoadAction(string triggerName, bool state)
        {
            if (animator != null)
            {
                animator.SetBool(triggerName, state);
            }
        }

        // ======================Item===========================

        [PunRPC]
        public void RPC_SizeDown()
        {
            transform.localScale *= 0.5f;
        }

        [PunRPC]
        public void RPC_SizeUp()
        {
            transform.localScale *= 2f;
        }

        [PunRPC]
        public void RPC_SizeReset()
        {
            transform.localScale = localSize;
        }

        [PunRPC]
        public void RPC_Magnet(float radius, float totalStr)
        {
            if (!photonView.IsMine) return;

            StartCoroutine(DoMagnet(radius, totalStr));
        }

        private System.Collections.IEnumerator DoMagnet(float radius, float totalStr)
        {
            float duration = 1.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
                foreach (Collider hit in colliders)
                {
                    if (hit.CompareTag("Player") && hit.gameObject != this.gameObject)
                    {
                        PhotonView targetPV = hit.GetComponent<PhotonView>();
                        if (targetPV != null)
                        {
                            Vector3 diff = transform.position - hit.transform.position;
                            // 아주 가까워지면(1m 이내) 더 이상 당기지 않음 (뒤로 넘어가는 것 방지)
                            if (diff.magnitude < 1.5f) continue;

                            Vector3 pullDirection = diff.normalized;

                            targetPV.RPC("RPC_AddKnockback", RpcTarget.All, pullDirection * (totalStr * Time.deltaTime));
                        }
                    }
                }
                elapsed += Time.deltaTime;
                yield return null; // 다음 프레임까지 대기
            }
        }
    }
}