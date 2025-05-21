using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerMovement : MonoBehaviour
{

    public float moveSpeed = 2.0f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    private CharacterController characterController;
    private float verticalVelocity = 0f;
    private bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // �ڒn����
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f; // �����ȉ������̒l�Œn�ʂɒ���t��
        }

        // �W�����v
        if (isGrounded && OVRInput.GetDown(OVRInput.RawButton.A))
        {
            verticalVelocity = jumpForce;
        }

        // �d�͓K�p
        verticalVelocity += gravity * Time.deltaTime;

        // ���ړ��i���X�e�B�b�N�j
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 move = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        move.y = 0f;
        move.Normalize(); // �΂߈ړ��������Ȃ�Ȃ��悤��

        // �������Ĉړ�
        Vector3 finalMove = move * moveSpeed + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }
}
