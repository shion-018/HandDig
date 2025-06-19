using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    private DigToolStats stats;
    private int upgradeLevel;

    public float digInterval = 0.1f;
    private Collider currentCollider;
    private float digTimer = 0f;

    public void SetStats(DigToolStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
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
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);

        if (currentCollider != null && triggerHeld)
        {
            digTimer += Time.deltaTime;
            if (digTimer >= digInterval && stats != null)
            {
                digTimer = 0f;

                float radius = stats.GetRadius(0, upgradeLevel); // comboStage = 0（ドリルは段階なし）
                digManager.DigAt(toolPosition, radius);
                Debug.Log($"[Drill] Dig at radius {radius}");
            }
        }
        else
        {
            digTimer = 0f;
        }
    }
}