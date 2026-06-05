using UnityEngine;

namespace Photon.Pun.UtilityScripts
{   
    public class MoveByKeys_FlameShot : MoveByKeys
    {
        private bool isFiring = false;
        private ParticleSystem[] flame;

        protected override void OnDisable()
        {
            if (photonView.IsMine && isFiring)
            {
                photonView.RPC("RPC_SetFlame", RpcTarget.All, false);
            }

            base.OnDisable();
        }

        protected override void HandleAttack()
        {
            if (isChatting() || isUIMode || isMenuOpen || isSleep) return;

            if (isLoadingAttack && isAttackPressed && !animator.GetCurrentAnimatorStateInfo(1).IsName("Attack"))
            {
                if (!isFiring)
                {
                    isFiring = true;
                    photonView.RPC("RPC_SetFlame", RpcTarget.All, true);
                }
            }
            else
            {
                if (isFiring)
                {
                    isFiring = false;
                    photonView.RPC("RPC_SetFlame", RpcTarget.All, false);
                }
            }

            if (isFiring && flame != null && photonView.IsMine)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                Vector3 targetPoint;

                if (Physics.Raycast(ray, out hit, maxRange, ~aimLayerMask))
                {
                    targetPoint = hit.point;

                    float distToTarget = Vector3.Distance(Camera.main.transform.position, hit.point);
                    float distToMuzzle = Vector3.Distance(Camera.main.transform.position, firePoint.position);

                    if (distToTarget < distToMuzzle + 5f)
                    {
                        targetPoint = ray.GetPoint(50f);
                    }
                }
                else targetPoint = ray.GetPoint(maxRange);

                Vector3 aimDirection = (targetPoint - firePoint.position).normalized;
                flame[0].transform.rotation = Quaternion.LookRotation(aimDirection);
            }
        }

        [PunRPC]
        public void RPC_SetFlame(bool state)
        {
            if (flame == null && state == true)
            {
                GameObject prefab = Resources.Load<GameObject>("Projectile/" + projectile);

                if (prefab != null && firePoint != null)
                {
                    GameObject flameObj = Instantiate(prefab, firePoint);

                    flameObj.transform.localPosition = Vector3.zero;
                    flameObj.transform.localRotation = Quaternion.identity;

                    flame = flameObj.GetComponentsInChildren<ParticleSystem>();
                }
                else
                {
                    Debug.LogError($"[FlameShot] Resources/Projectile/{projectile} 을 찾을 수 없거나 firePoint가 비어있습니다!");
                }
            }

            if (flame != null)
            {
                foreach (var ps in flame)
                {
                    var emission = ps.emission;
                    emission.enabled = state;

                    if (state && !ps.isPlaying)
                    {
                        ps.Play();
                    }
                }
            }
        }
    }
}

