using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public DigToolStats stats; // ← アセットをインスペクタから指定

    private Collider currentHitCollider = null;

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

        if (currentHitCollider != null && (isTriggerPressed || isSpacePressed))
        {
            float radius = stats.GetRadius(); // comboStageなし
            digManager.DigAt(toolPosition, radius);
            Debug.Log($"[HandDig] 掘ったよ！ Radius: {radius}");
        }
    }
}