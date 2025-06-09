using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public float baseRadius = 1.5f;
    public float stage2Radius = 2.0f;
    public float stage3Radius = 2.5f;

    public float minComboTime = 0.5f;
    public float maxComboTime = 1.5f;

    private Collider currentCollider;
    private float lastDigTime = -10f;
    private int comboStage = 0;

    private bool isSwingReady = false;

    public void SetSwingReady(bool ready)
    {
        isSwingReady = ready;
        Debug.Log($"[Pickaxe] SwingReady = {ready}");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;

            // スイング準備ができている + 入力中 なら掘る
            bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            bool isSpaceHeld = Input.GetKey(KeyCode.Space);

            if (isSwingReady && (isTriggerHeld || isSpaceHeld))
            {
                float currentTime = Time.time;
                float timeSinceLast = currentTime - lastDigTime;

                if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
                    comboStage = Mathf.Min(comboStage + 1, 2);
                else
                    comboStage = 0;

                lastDigTime = currentTime;
                isSwingReady = false; // 掘ったらリセット

                float radius = comboStage switch
                {
                    1 => stage2Radius,
                    2 => stage3Radius,
                    _ => baseRadius
                };

                digManager.DigAt(transform.position, radius);
                Debug.Log($"[Pickaxe] Combo {comboStage + 1} / radius: {radius}");
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentCollider == other)
            currentCollider = null;
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        // 速度・相対移動などは使わないのでここは空のままでOK
    }
}
