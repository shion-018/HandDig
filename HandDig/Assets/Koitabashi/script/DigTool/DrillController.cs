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

    private bool prevTrigger;
    private bool isDrilling;

    void Update()
    {
        bool trigger = GetInput();

        animator.SetBool("IsTriggerHeld", trigger);

        // âüÇµÇΩèuä‘
        if (trigger && !prevTrigger)
        {
            animator.SetTrigger("Start");
            StartDrill();
        }

        // ó£ÇµÇΩèuä‘
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

        // âπÇÃëΩèdçƒê∂ñhé~
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
