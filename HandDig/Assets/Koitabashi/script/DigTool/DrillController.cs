using UnityEngine;

public class DrillController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Motor Sounds")]
    [SerializeField] private AudioSource startSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource endSource;

    [Header("Input (Debug)")]
    [SerializeField] private bool enableKeyboardInput = true;

    [Header("Start Sound Delay")]
    [SerializeField] private float loopStartDelay = 0.3f;

    [Header("Tool Reference")]
    [Tooltip("ドリルツールスクリプト（モード確認用）")]
    [SerializeField] private DrillDigTool drillDigTool;

    private bool prevTrigger;
    private bool isDrilling;

    private void Start()
    {
        // DrillDigToolが見つからない場合は自動検索
        if (drillDigTool == null)
        {
            drillDigTool = GetComponentInParent<DrillDigTool>();
            if (drillDigTool == null)
            {
                drillDigTool = FindObjectOfType<DrillDigTool>();
            }
        }
    }

    void Update()
    {
        bool trigger = GetInput();

        animator.SetBool("IsTriggerHeld", trigger);

        // 射出モードの場合は音を鳴らさない
        bool isShootMode = drillDigTool != null && drillDigTool.IsShootMode();
        
        // トリガー開始
        if (trigger && !prevTrigger)
        {
            animator.SetTrigger("Start");
            if (!isShootMode)
            {
                StartDrill();
            }
        }

        // トリガー終了
        if (!trigger && prevTrigger)
        {
            animator.SetTrigger("End");
            if (!isShootMode)
            {
                StopDrill();
            }
            else
            {
                // 射出モード時でも音を停止（念のため）
                StopDrill();
            }
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

        // 前回の音を停止
        if (endSource.isPlaying) endSource.Stop();
        if (startSource.isPlaying) startSource.Stop();

        startSource.Play();
        Invoke(nameof(PlayLoop), loopStartDelay);
    }

    void PlayLoop()
    {
        if (!isDrilling) return;

        loopSource.loop = true;
        if (!loopSource.isPlaying)
            loopSource.Play();
    }

    void StopDrill()
    {
        isDrilling = false;

        if (startSource.isPlaying) startSource.Stop();
        if (loopSource.isPlaying) loopSource.Stop();

        endSource.Play();
    }
}
