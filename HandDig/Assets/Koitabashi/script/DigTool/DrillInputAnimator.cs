using UnityEngine;

public class DrillInputAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("デバッグ用（エディタ操作）")]
    [SerializeField] private bool enableKeyboardInput = true;

    private bool prevTrigger;

    void Update()
    {
        bool trigger = GetInput();

        animator.SetBool("IsTriggerHeld", trigger);

        // 押した瞬間
        if (trigger && !prevTrigger)
        {
            animator.SetTrigger("Start");
        }

        // 離した瞬間
        if (!trigger && prevTrigger)
        {
            animator.SetTrigger("End");
        }

        prevTrigger = trigger;
    }

    bool GetInput()
    {
        bool vrInput = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        bool keyInput = enableKeyboardInput && Input.GetKey(KeyCode.Space);

        return vrInput || keyInput;
    }
}
