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

    [Header("ドリルツール参照")]
    [Tooltip("DrillDigToolへの参照（自動検索される）")]
    private DrillDigTool drillDigTool;

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
    }

    void Update()
    {
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

        // ���̑��d�Đ��h�~
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

    /// <summary>
    /// 射出モードかどうかを判定
    /// </summary>
    private bool IsShootMode()
    {
        if (drillDigTool == null) return false;
        return drillDigTool.IsShootMode();
    }
}
