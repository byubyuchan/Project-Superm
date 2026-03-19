using Photon.Pun;
using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요

public class Turning : MonoBehaviourPun
{
    public float rotationSpeed = 90f;

    private List<CharacterController> playersOnPlatform = new List<CharacterController>();

    void FixedUpdate()
    {
        // 다리는 모든 클라이언트에서 동일하게 돌아갑니다.

        if (PhotonNetwork.IsMasterClient)
        {
            transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
        }

        for (int i = playersOnPlatform.Count - 1; i >= 0; i--)
        {
            CharacterController player = playersOnPlatform[i];

            if (player != null && player.enabled)
            {
                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    float angle = rotationSpeed * Time.fixedDeltaTime;

                    Vector3 offset = player.transform.position - transform.position;
                    Vector3 rotatedOffset = Quaternion.Euler(0, angle, 0) * offset;
                    Vector3 moveDirection = rotatedOffset - offset;

                    if (player.isGrounded)
                    {
                        moveDirection.y = -2f;
                    }

                    player.Move(moveDirection);
                }
            }
            else
            {
                playersOnPlatform.RemoveAt(i);
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterController cc = collision.gameObject.GetComponent<CharacterController>();
            if (cc != null && !playersOnPlatform.Contains(cc))
            {
                playersOnPlatform.Add(cc);
                Debug.Log("Player added to platform: " + collision.gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CharacterController cc = collision.gameObject.GetComponent<CharacterController>();
            if (cc != null && playersOnPlatform.Contains(cc))
            {
                playersOnPlatform.Remove(cc);
            }
        }
    }
}