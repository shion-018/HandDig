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
    public float swingThreshold = 1.5f; // 振り下ろしの速度しきい値

    private Collider currentCollider;
    private float lastDigTime = -10f;
    private int comboStage = 0;

    private Vector3 previousPosition;
    private float velocity;

    void Start()
    {
        previousPosition = transform.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            currentCollider = other;
    }

    public void OnTriggerExit(Collider other)
    {
        if (currentCollider == other)
            currentCollider = null;
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        if (currentCollider == null) return;

        // 入力確認：VRトリガー or スペースキー
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);
        if (!isTriggerHeld && !isSpaceHeld) return;

        // 移動速度を計算（フレーム間位置差分）
        velocity = (toolPosition - previousPosition).magnitude / Time.deltaTime;
        previousPosition = toolPosition;

        if (velocity >= swingThreshold)
        {
            float currentTime = Time.time;
            float timeSinceLast = currentTime - lastDigTime;

            // コンボ段階更新
            if (timeSinceLast >= minComboTime && timeSinceLast <= maxComboTime)
                comboStage = Mathf.Min(comboStage + 1, 2);
            else
                comboStage = 0;

            lastDigTime = currentTime;

            float radius = comboStage switch
            {
                1 => stage2Radius,
                2 => stage3Radius,
                _ => baseRadius
            };

            digManager.DigAt(toolPosition, radius);
            Debug.Log($"Pickaxe: Combo {comboStage + 1} / velocity: {velocity:F2} / radius: {radius}");
        }
    }
}