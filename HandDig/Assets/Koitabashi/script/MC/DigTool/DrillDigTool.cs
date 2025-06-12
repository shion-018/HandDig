using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public DigToolStats stats;

    public float digInterval = 0.1f;
    private Collider currentCollider;
    private float digTimer = 0f;

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
            if (digTimer >= digInterval)
            {
                digTimer = 0f;
                float radius = stats.GetRadius();
                digManager.DigAt(toolPosition, radius);
            }
        }
        else
        {
            digTimer = 0f;
        }
    }
}