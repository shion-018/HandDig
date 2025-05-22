using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRPlayerMovement : MonoBehaviour
{
    public enum TurnMode { Smooth, Snap }

    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    [Header("Turn Settings")]
    public TurnMode turnMode = TurnMode.Snap;
    public float smoothTurnSpeed = 60f;
    public float snapTurnAngle = 45f;
    public float snapInputThreshold = 0.8f;

    private CharacterController characterController;
    private float verticalVelocity = 0f;
    private bool isGrounded;
    private bool canSnapTurn = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleTurning();
    }

    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -1f;

        if (isGrounded && OVRInput.GetDown(OVRInput.RawButton.A))
            verticalVelocity = jumpForce;

        verticalVelocity += gravity * Time.deltaTime;

        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 move = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        move.y = 0f;
        move.Normalize();

        Vector3 finalMove = move * moveSpeed + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

    void HandleTurning()
    {
        float rightX = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        if (turnMode == TurnMode.Smooth)
        {
            if (Mathf.Abs(rightX) > 0.1f)
                transform.Rotate(Vector3.up, rightX * smoothTurnSpeed * Time.deltaTime);
        }
        else if (turnMode == TurnMode.Snap)
        {
            if (canSnapTurn)
            {
                if (rightX > snapInputThreshold)
                {
                    transform.Rotate(Vector3.up, snapTurnAngle);
                    canSnapTurn = false;
                }
                else if (rightX < -snapInputThreshold)
                {
                    transform.Rotate(Vector3.up, -snapTurnAngle);
                    canSnapTurn = false;
                }
            }
            if (Mathf.Abs(rightX) < 0.2f)
                canSnapTurn = true;
        }
    }
}
