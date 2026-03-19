using Photon.Pun;
using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요

public class Turning : MonoBehaviourPun
{
    public float rotationSpeed = 90f;

    private List<CharacterController> playersOnPlatform = new List<CharacterController>();

    void FixedUpdate()
    {

        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        foreach (CharacterController player in playersOnPlatform)
        {
            if (player != null && player.enabled)
            {
                Vector3 offset = player.transform.position - transform.position;

                Vector3 rotatedOffset = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0) * offset;

                Vector3 moveDirection = rotatedOffset - offset;

                if (player.isGrounded)
                {
                    moveDirection.y = -2f;
                }

                player.Move(moveDirection);
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