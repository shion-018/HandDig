using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRDigTool : MonoBehaviour,IDigTool
{
    public VoxelDigManager digManager;
    public float digRadius = 2f;

    private Collider currentHitCollider = null;
    private bool canDig = true;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentHitCollider = other;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == currentHitCollider)
        {
            currentHitCollider = null;
        }
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        bool isTriggerPressed = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger);
        bool isSpacePressed = Input.GetKeyDown(KeyCode.Space);

        if (currentHitCollider != null && (isTriggerPressed || isSpacePressed))
        {
            digManager.DigAt(toolPosition, digRadius);
            Debug.Log($"HandDigTool: å@Ç¡ÇΩÇÊÅIà íu: {toolPosition}");
        }
    }
}