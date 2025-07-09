using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRDigTool : MonoBehaviour, IDigToolWithStats
{
    public VoxelDigManager digManager;

    private DigToolStats stats;
    private int upgradeLevel;

    private Collider currentHitCollider = null;

    public void SetStats(DigToolStats newStats, int level)
    {
        stats = newStats;
        upgradeLevel = level;
    }

    public void SetHandStats(HandDigStats newStats, int level)
    {
        // Hand用なので未実装
    }

    public void SetPickaxeStats(PickaxeDigStats newStats, int level)
    {
        // Pickaxe用なので未実装
    }

    public void SetDrillStats(DrillDigStats newStats, int level)
    {
        // Drill用なので未実装
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            currentHitCollider = other;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == currentHitCollider)
            currentHitCollider = null;
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        bool isTriggerPressed = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
        bool isSpacePressed = Input.GetKeyDown(KeyCode.Space);

        if (currentHitCollider != null && (isTriggerPressed || isSpacePressed) && stats != null)
        {
            float radius = stats.GetRadius(0, upgradeLevel); // comboStage = 0（コンボ段階なし）
            digManager.DigAt(toolPosition, radius);
            Debug.Log($"[HandDig] 掘削実行！ Radius: {radius}");
        }
    }
}
