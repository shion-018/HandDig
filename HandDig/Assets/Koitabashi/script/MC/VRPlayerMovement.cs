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
        // 接地判定
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f; // 小さな下向きの値で地面に張り付く
        }

        // ジャンプ
        if (isGrounded && OVRInput.GetDown(OVRInput.RawButton.A))
        {
            verticalVelocity = jumpForce;
        }

        // 重力適用
        verticalVelocity += gravity * Time.deltaTime;

        // 横移動（左スティック）
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 move = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        move.y = 0f;
        move.Normalize(); // 斜め移動が速くならないように

        // 合成して移動
        Vector3 finalMove = move * moveSpeed + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }
}
