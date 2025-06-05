using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillDigTool : MonoBehaviour, IDigTool
{
    public VoxelDigManager digManager;
    public float drillRadius = 1.0f;      // �@��͈́i�T���߁j
    public float digInterval = 0.1f;      // �@��Ԋu�i�A���I�j

    private Collider currentCollider;
    private float digTimer = 0f;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentCollider = other;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == currentCollider)
        {
            currentCollider = null;
        }
    }

    public void UpdateDig(Vector3 toolPosition)
    {
        // �ڐG�� && �g���K�[�����Ă� or �X�y�[�X�L�[
        bool triggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space);
        if (currentCollider != null && triggerHeld)
        {
            digTimer += Time.deltaTime;
            if (digTimer >= digInterval)
            {
                digTimer = 0f;
                digManager.DigAt(toolPosition, drillRadius);
            }
        }
        else
        {
            digTimer = 0f; // �@���ĂȂ����̓��Z�b�g
        }
    }
}