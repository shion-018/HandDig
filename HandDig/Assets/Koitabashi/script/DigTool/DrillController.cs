using System.Collections;
using UnityEngine;

public class DrillController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Motor Sounds (旧方式 - 互換性のため残しています)")]
    [SerializeField] private AudioSource startSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource endSource;
    
    [Header("音声管理")]
    [Tooltip("サウンドマネージャーを使用するか（true: 新方式、false: 旧方式）")]
    [SerializeField] private bool useSoundManager = true;

    [Header("Input (Debug)")]
    [SerializeField] private bool enableKeyboardInput = true;

    [Header("Start Sound Delay")]
    [SerializeField] private float loopStartDelay = 0.3f;

    [Header("ドリルツール参照")]
    [Tooltip("DrillDigToolへの参照（自動検索される）")]
    private DrillDigTool drillDigTool;
    
    [Header("音声マネージャー参照")]
    private DigSoundManager soundManager;
    
    // 新方式で使用するAudioSourceの参照
    private AudioSource currentLoopAudioSource;

    private bool prevTrigger;
    private bool isDrilling;

    private void Start()
    {
        // DrillDigToolを自動検索（親オブジェクトから）
        if (drillDigTool == null)
        {
            drillDigTool = GetComponentInParent<DrillDigTool>();
        }
        // 見つからない場合はシーン全体から検索
        if (drillDigTool == null)
        {
            drillDigTool = FindObjectOfType<DrillDigTool>();
        }
        
        // サウンドマネージャーを取得
        if (useSoundManager)
        {
            soundManager = DigSoundManager.Instance;
        }
    }
    
    /// <summary>
    /// オブジェクトが有効になった時
    /// </summary>
    private void OnEnable()
    {
        // DrillDigToolを再検索（OnEnableはStartより先に呼ばれる可能性がある）
        if (drillDigTool == null)
        {
            drillDigTool = GetComponentInParent<DrillDigTool>();
            if (drillDigTool == null)
            {
                drillDigTool = FindObjectOfType<DrillDigTool>();
            }
        }
        
        // サウンドマネージャーを取得
        if (useSoundManager && soundManager == null)
        {
            soundManager = DigSoundManager.Instance;
        }
        
        // トリガーが既に押されている状態でドリルに切り替えた場合
        // ただし、射出モードの時はスキップ
        if (drillDigTool != null && !IsShootMode())
        {
            bool trigger = GetInput();
            if (trigger)
            {
                // トリガーが押されているので、音とアニメーションを開始
                prevTrigger = true; // 前回の状態を記録（Updateで反応しないように）
                isDrilling = false; // 一度リセット
                
                // 次のフレームで開始（OnEnable内で直接呼ぶと問題が起きる可能性があるため）
                StartCoroutine(StartDrillOnNextFrame());
            }
            else
            {
                prevTrigger = false;
            }
        }
    }
    
    /// <summary>
    /// 次のフレームでドリルを開始（OnEnable内で直接呼ぶと問題が起きる可能性があるため）
    /// </summary>
    private System.Collections.IEnumerator StartDrillOnNextFrame()
    {
        yield return null; // 1フレーム待つ
        
        // 再度チェック（射出モードに変わった可能性があるため）
        if (drillDigTool != null && !IsShootMode() && GetInput())
        {
            if (animator != null)
            {
                animator.SetBool("IsTriggerHeld", true);
                animator.SetTrigger("Start");
            }
            StartDrill();
        }
    }
    
    /// <summary>
    /// オブジェクトが無効になった時（ツール切り替え時など）
    /// </summary>
    private void OnDisable()
    {
        // ツールが非アクティブになった時は音を停止
        if (isDrilling)
        {
            StopDrill();
        }
        
        // 予約されたInvokeをキャンセル
        CancelInvoke(nameof(PlayLoop));
    }

    void Update()
    {
        // DrillDigToolが存在しない場合は処理をスキップ（ピッケルなど他のツールの時）
        if (drillDigTool == null)
        {
            if (isDrilling)
            {
                StopDrill();
            }
            return;
        }
        
        // ドリルツールが非アクティブの場合は処理をスキップ（ツール切り替え時）
        if (!drillDigTool.gameObject.activeInHierarchy)
        {
            if (isDrilling)
            {
                StopDrill();
            }
            return;
        }
        
        // このGameObjectが非アクティブの場合は処理をスキップ
        if (!gameObject.activeInHierarchy)
        {
            if (isDrilling)
            {
                StopDrill();
            }
            return;
        }
        
        // 射出モードの時は通常掘り用の音を鳴らさない
        if (IsShootMode())
        {
            if (isDrilling)
            {
                StopDrill();
            }
            return;
        }

        bool trigger = GetInput();

        animator.SetBool("IsTriggerHeld", trigger);

        // �������u��
        if (trigger && !prevTrigger)
        {
            animator.SetTrigger("Start");
            StartDrill();
        }

        // �������u��
        if (!trigger && prevTrigger)
        {
            animator.SetTrigger("End");
            StopDrill();
        }

        prevTrigger = trigger;
    }

    bool GetInput()
    {
        bool vrInput = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool keyInput = enableKeyboardInput && Input.GetKey(KeyCode.Space);
        return vrInput || keyInput;
    }

    void StartDrill()
    {
        isDrilling = true;

        if (useSoundManager && soundManager != null)
        {
            // 新方式：サウンドマネージャーを使用
            // 終了音を停止
            soundManager.StopDrillMotorSounds(transform);
            
            // 開始音を再生
            soundManager.PlayDrillMotorStartSound(transform);
            
            // ループ音を遅延再生
            Invoke(nameof(PlayLoop), loopStartDelay);
        }
        else
        {
            // 旧方式：直接AudioSourceを使用
            if (endSource != null && endSource.isPlaying) endSource.Stop();
            if (startSource != null && startSource.isPlaying) startSource.Stop();

            if (startSource != null)
            {
                startSource.Play();
            }
            Invoke(nameof(PlayLoop), loopStartDelay);
        }
    }

    void PlayLoop()
    {
        if (!isDrilling) return;

        if (useSoundManager && soundManager != null)
        {
            // 新方式：サウンドマネージャーを使用
            currentLoopAudioSource = soundManager.PlayDrillMotorLoopSound(transform);
        }
        else
        {
            // 旧方式：直接AudioSourceを使用
            if (loopSource != null)
            {
                loopSource.loop = true;
                if (!loopSource.isPlaying)
                    loopSource.Play();
            }
        }
    }

    void StopDrill()
    {
        isDrilling = false;

        if (useSoundManager && soundManager != null)
        {
            // 新方式：サウンドマネージャーを使用
            // 開始音とループ音を停止
            soundManager.StopDrillMotorSounds(transform);
            
            // 終了音を再生
            soundManager.PlayDrillMotorEndSound(transform);
        }
        else
        {
            // 旧方式：直接AudioSourceを使用
            if (startSource != null && startSource.isPlaying) startSource.Stop();
            if (loopSource != null && loopSource.isPlaying) loopSource.Stop();

            if (endSource != null)
            {
                endSource.Play();
            }
        }
    }

    /// <summary>
    /// 射出モードかどうかを判定
    /// </summary>
    private bool IsShootMode()
    {
        if (drillDigTool == null) return false;
        return drillDigTool.IsShootMode();
    }
}
