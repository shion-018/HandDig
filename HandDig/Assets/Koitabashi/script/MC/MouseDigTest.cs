using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDigTest : MonoBehaviour
{
    public VoxelDigManager digManager;
    public LayerMask terrainLayer;
    private Vector3? lastHitPoint = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, terrainLayer))
            {
                digManager.DigAt(hit.point);
                lastHitPoint = hit.point;
            }
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit2, 100f, terrainLayer))
            lastHitPoint = hit2.point;
        else
            lastHitPoint = null;
    }

    void OnDrawGizmos()
    {
        if (lastHitPoint.HasValue)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lastHitPoint.Value, digManager.digRadius);
        }
    }
}

