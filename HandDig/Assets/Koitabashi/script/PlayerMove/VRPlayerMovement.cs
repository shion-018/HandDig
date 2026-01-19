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
    
    [Header("足音設定")]
    [Tooltip("足音を有効にするか")]
    public bool enableFootsteps = true;
    
    [Tooltip("足音の最小移動速度（これ以下では足音を再生しない）")]
    public float minMoveSpeedForFootsteps = 0.1f;
    
    [Header("Locomotion Tunneling設定")]
    [Tooltip("LocomotionTunnelingコンポーネント（自動検索も可能）")]
    [SerializeField] private LocomotionTunneling locomotionTunneling;
    
    [Header("ジェットパック音設定")]
    [Tooltip("ジェットパック音の再生位置（nullの場合はこのTransformを使用）")]
    [SerializeField] private Transform jetpackSoundPosition;
    
    [Tooltip("ジェットパック音量のフェード時間（秒）")]
    [Range(0.1f, 2f)]
    [SerializeField] private float jetpackVolumeFadeDuration = 0.3f;
    
    private Vector3 lastPosition;
    private float distanceTraveled = 0f;
    
    // 移動速度を外部から取得できるようにする
    private float currentMoveSpeed = 0f;
    
    // ジェットパック音の管理
    private AudioSource jetpackAudioSource;
    private bool wasJetpackActive = false;
    private bool jetpackSoundStarted = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastPosition = transform.position;
        
        // LocomotionTunnelingを自動検索
        if (locomotionTunneling == null)
        {
            locomotionTunneling = FindObjectOfType<LocomotionTunneling>();
        }
        
        // ジェットパック音の再生位置を設定
        if (jetpackSoundPosition == null)
        {
            jetpackSoundPosition = transform;
        }
        
        // ジェットパック音は地形生成完了後に開始（Updateでチェック）
    }
    
    private void OnDestroy()
    {
        // ジェットパック音を停止
        if (DigSoundManager.Instance != null && jetpackSoundPosition != null)
        {
            DigSoundManager.Instance.StopJetpackSound(jetpackSoundPosition);
        }
    }

    void Update()
    {
        // 地形生成中は操作を無効化
        if (IsTerrainGenerating())
        {
            return;
        }

        // 地形生成が完了したらジェットパック音を開始（一度だけ）
        if (!jetpackSoundStarted)
        {
            StartJetpackSound();
            jetpackSoundStarted = true;
        }

        HandleMovement();
        HandleTurning();
    }

    /// <summary>
    /// 地形生成中かどうかを判定
    /// </summary>
    private bool IsTerrainGenerating()
    {
        MC_World world = FindObjectOfType<MC_World>();
        if (world != null)
        {
            return !world.IsInitialized;
        }
        return false;
    }
    void HandleMovement()//�z��������
    {
        isGrounded = characterController.isGrounded;

        // 移動入力�㏸����
        bool isAButtonHeld = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);

        float maxAscendSpeed = 5f;
        float ascendAcceleration = 10f;
        float descendAcceleration = 15f;

        if (isAButtonHeld)
        {
            // 移動入力�����㏸
            verticalVelocity += ascendAcceleration * Time.deltaTime;
            verticalVelocity = Mathf.Clamp(verticalVelocity, 0f, maxAscendSpeed);
        }
        else
        {
            // 移動入力�d�͉��Z�i�~���j
            verticalVelocity += gravity * descendAcceleration * Time.deltaTime;
        }

        // 移動入力�n�ʂɂ����Ԃŗ������x�����Z�b�g
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

        // 移動入力�ړ�����
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 move = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        move.y = 0f;
        move.Normalize();

        // 移動入力�����ړ�
        Vector3 finalMove = move * moveSpeed + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
        
        // 足音の処理
        if (enableFootsteps)
        {
            HandleFootsteps(move.magnitude);
        }
        
        // ジェットパック音の処理
        HandleJetpackSound(isAButtonHeld);
    }
    
    /// <summary>
    /// ジェットパック音を開始
    /// </summary>
    private void StartJetpackSound()
    {
        if (DigSoundManager.Instance != null && jetpackSoundPosition != null)
        {
            jetpackAudioSource = DigSoundManager.Instance.PlayJetpackSound(jetpackSoundPosition, false);
            wasJetpackActive = false;
        }
    }
    
    /// <summary>
    /// ジェットパック音を処理
    /// </summary>
    private void HandleJetpackSound(bool isActive)
    {
        // ジェットパック音がまだ開始されていない場合は開始
        if (jetpackAudioSource == null && DigSoundManager.Instance != null && jetpackSoundPosition != null)
        {
            StartJetpackSound();
        }
        
        // 状態が変わった場合のみ更新
        if (isActive != wasJetpackActive && jetpackAudioSource != null && DigSoundManager.Instance != null)
        {
            DigSoundManager.Instance.UpdateJetpackVolume(jetpackAudioSource, isActive, jetpackVolumeFadeDuration);
            wasJetpackActive = isActive;
        }
    }
    
    /// <summary>
    /// 足音を処理する
    /// </summary>
    /// <param name="moveSpeed">現在の移動速度（0-1）</param>
    void HandleFootsteps(float moveSpeed)
    {
        // 地面に接地していない、または移動速度が低い場合は足音を再生しない
        if (!isGrounded || moveSpeed < minMoveSpeedForFootsteps)
        {
            distanceTraveled = 0f;
            return;
        }
        
        // 移動距離を累積
        float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
        distanceTraveled += distanceThisFrame;
        
        // 現在の移動速度を記録（LocomotionTunneling用）
        currentMoveSpeed = distanceThisFrame / Time.deltaTime;
        
        // 足音設定を取得
        var soundManager = DigSoundManager.Instance;
        if (soundManager != null && soundManager.soundSettings != null)
        {
            float footstepDistance = soundManager.soundSettings.footstepDistance;
            
            // 一定距離移動したら足音を再生
            if (distanceTraveled >= footstepDistance)
            {
                soundManager.PlayFootstepSound(transform.position);
                distanceTraveled = 0f; // リセット
            }
        }
        
        lastPosition = transform.position;
    }
    /*
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
    */
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
