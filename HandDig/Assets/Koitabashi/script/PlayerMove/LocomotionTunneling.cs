using UnityEngine;
using System.Reflection;

/// <summary>
/// 移動時と視点移動時にOVR Vignette（トンネリング）エフェクトを適用するクラス
/// </summary>
public class LocomotionTunneling : MonoBehaviour
{
    [Header("OVR Vignette設定")]
    [Tooltip("OVR Vignetteコンポーネント（自動検索も可能）")]
    [SerializeField] private MonoBehaviour vignette;
    
    [Tooltip("移動時のVignette最大強度（0-1）")]
    [Range(0f, 1f)]
    [SerializeField] private float maxMoveVignette = 0.6f;
    
    [Header("速度閾値設定")]
    [Tooltip("移動速度の閾値（これ以上でVignetteが適用される）")]
    [SerializeField] private float moveSpeedThreshold = 0.15f;
    
    [Header("フェード設定")]
    [Tooltip("Vignetteのフェードイン速度")]
    [SerializeField] private float fadeInSpeed = 5f;
    
    [Tooltip("Vignetteのフェードアウト速度")]
    [SerializeField] private float fadeOutSpeed = 3f;
    
    [Header("参照設定")]
    [Tooltip("移動速度を取得するVRPlayerMovement（自動検索も可能）")]
    [SerializeField] private VRPlayerMovement playerMovement;
    
    [Tooltip("カメラTransform（回転速度計算用、自動検索も可能）")]
    [SerializeField] private Transform cameraTransform;
    
    private float currentVignetteAlpha = 0f;
    private Vector3 lastCameraPosition;
    private float currentMoveSpeed = 0f;
    
    private void Start()
    {
        // OVR Vignetteを自動検索
        if (vignette == null)
        {
            // OVRCameraRigを探す
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                // OVRCameraRigのVignetteプロパティを取得
                // OVRCameraRigにはVignetteプロパティがある可能性がある
                var cameraRigType = cameraRig.GetType();
                var vignetteProperty = cameraRigType.GetProperty("Vignette");
                if (vignetteProperty != null)
                {
                    vignette = vignetteProperty.GetValue(cameraRig) as MonoBehaviour;
                }
                
                // プロパティが見つからない場合は、カメラオブジェクトから取得を試みる
                if (vignette == null && cameraRig.centerEyeAnchor != null)
                {
                    Camera centerEyeCamera = cameraRig.centerEyeAnchor.GetComponent<Camera>();
                    if (centerEyeCamera != null)
                    {
                        // "Vignette"という名前のコンポーネントを探す
                        Component[] components = centerEyeCamera.GetComponents<Component>();
                        foreach (var comp in components)
                        {
                            if (comp != null && comp.GetType().Name.Contains("Vignette"))
                            {
                                vignette = comp as MonoBehaviour;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        // 自動検索
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<VRPlayerMovement>();
        }
        
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                // OVR Camera Rigを探す
                OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
                if (cameraRig != null && cameraRig.centerEyeAnchor != null)
                {
                    cameraTransform = cameraRig.centerEyeAnchor;
                }
            }
        }
        
        // 初期位置を記録
        if (cameraTransform != null)
        {
            lastCameraPosition = cameraTransform.position;
        }
        
        // 初期状態ではVignetteを無効化
        if (vignette != null)
        {
            vignette.enabled = false;
        }
    }
    
    private void Update()
    {
        if (vignette == null || cameraTransform == null)
        {
            return;
        }
        
        // 移動速度を計算
        CalculateMoveSpeed();
        
        // 必要なVignette強度を計算
        float targetVignette = CalculateTargetVignette();
        
        // Vignetteを更新
        UpdateVignette(targetVignette);
    }
    
    /// <summary>
    /// 移動速度を計算
    /// </summary>
    private void CalculateMoveSpeed()
    {
        if (cameraTransform == null)
        {
            currentMoveSpeed = 0f;
            return;
        }
        
        Vector3 currentPosition = cameraTransform.position;
        float distance = Vector3.Distance(currentPosition, lastCameraPosition);
        currentMoveSpeed = distance / Time.deltaTime;
        
        lastCameraPosition = currentPosition;
    }
    
    /// <summary>
    /// 目標Vignette強度を計算（スティック入力のみ）
    /// </summary>
    private float CalculateTargetVignette()
    {
        // スティック入力による移動を検知（左スティックのみ）
        Vector2 stickL = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        float stickMagnitude = stickL.magnitude;
        bool isMovingByStick = stickMagnitude > moveSpeedThreshold;
        
        // スティック入力のみでVignetteを適用
        if (isMovingByStick)
        {
            // スティック入力の強度を正規化（0-1）
            float normalizedStickIntensity = Mathf.Clamp01((stickMagnitude - moveSpeedThreshold) / (1f - moveSpeedThreshold));
            return normalizedStickIntensity * maxMoveVignette;
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Vignetteを更新
    /// </summary>
    private void UpdateVignette(float targetAlpha)
    {
        if (vignette == null)
        {
            return;
        }
        
        // フェードイン/フェードアウト
        float fadeSpeed = targetAlpha > currentVignetteAlpha ? fadeInSpeed : fadeOutSpeed;
        currentVignetteAlpha = Mathf.MoveTowards(currentVignetteAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        
        // OVR Vignetteの有効/無効を切り替え
        // 閾値以上の場合のみ有効化
        bool shouldBeEnabled = currentVignetteAlpha > 0.01f;
        
        // enabledプロパティをリフレクションで設定
        var enabledProperty = vignette.GetType().GetProperty("enabled");
        if (enabledProperty != null)
        {
            bool currentEnabled = (bool)enabledProperty.GetValue(vignette);
            if (currentEnabled != shouldBeEnabled)
            {
                enabledProperty.SetValue(vignette, shouldBeEnabled);
            }
        }
        else
        {
            // enabledプロパティがない場合は、MonoBehaviourのenabledを使用
            if (vignette.enabled != shouldBeEnabled)
            {
                vignette.enabled = shouldBeEnabled;
            }
        }
        
        // Vignetteの強度を設定（OVR VignetteのVignetteIntensityプロパティがある場合）
        if (shouldBeEnabled)
        {
            SetVignetteIntensity(currentVignetteAlpha);
        }
    }
    
    /// <summary>
    /// Vignetteの強度を設定
    /// </summary>
    private void SetVignetteIntensity(float intensity)
    {
        if (vignette == null) return;
        
        // OVR Vignetteの強度を設定
        // リフレクションを使用してVignetteIntensityプロパティにアクセス
        var vignetteType = vignette.GetType();
        var intensityProperty = vignetteType.GetProperty("VignetteIntensity");
        if (intensityProperty != null)
        {
            intensityProperty.SetValue(vignette, intensity, null);
        }
        else
        {
            // VignetteIntensityプロパティがない場合は、フィールドを探す
            var intensityField = vignetteType.GetField("vignetteIntensity", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (intensityField != null)
            {
                intensityField.SetValue(vignette, intensity);
            }
        }
    }
    
    /// <summary>
    /// デバッグ用：現在の移動速度を取得
    /// </summary>
    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }
}
