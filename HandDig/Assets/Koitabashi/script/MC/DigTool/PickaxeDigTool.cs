using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public DigToolStats stats;

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
                isSwingReady = false;

                float radius = stats.GetRadius(comboStage);

                Vector3 upwardOffset = transform.up * (radius * 0.3f);
                Vector3 digPosition = transform.position + upwardOffset;

                digManager.DigAt(digPosition, radius);
                Debug.Log($"[Pickaxe] Combo {comboStage + 1} / radius: {radius} / offset Y: {upwardOffset.y:F2}");
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
        // �������OK
    }
}