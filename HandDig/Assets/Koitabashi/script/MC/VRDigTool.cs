using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRDigTool : MonoBehaviour
{
    public VoxelDigManager digManager;
    public float digInterval = 0.1f; // Œ@‚éŠÔŠu
    public float digRadius = 2f;

    private float digTimer = 0f;
    private Collider currentHitCollider = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            currentHitCollider = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other == currentHitCollider)
        {
            currentHitCollider = null;
        }
    }

    void Update()
    {
        if (currentHitCollider != null && OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            digTimer += Time.deltaTime;
            if (digTimer >= digInterval)
            {
                digTimer = 0f;
                Vector3 digPoint = transform.position;
                digManager.DigAt(digPoint);
            }
        }
        else
        {
            digTimer = 0f;
        }
    }
}