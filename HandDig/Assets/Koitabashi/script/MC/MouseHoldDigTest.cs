using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHoldDigTest : MonoBehaviour
{
    public VoxelDigManager digManager;
    public LayerMask terrainLayer;
    public float digInterval = 0.1f; // 掘る間隔（秒）

    private float digTimer = 0f;
    private Vector3? lastHitPoint = null;

    void Update()
    {
        // Raycastで掘る場所を取得
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, terrainLayer))
        {
            lastHitPoint = hit.point;

            // 左クリック押しっぱなしで一定間隔ごとに掘る
            if (Input.GetMouseButton(0))
            {
                digTimer += Time.deltaTime;
                if (digTimer >= digInterval)
                {
                    digManager.DigAt(hit.point);
                    digTimer = 0f;
                }
            }
            else
            {
                digTimer = digInterval; // 押してない間は次回すぐ掘れるようにしておく
            }
        }
        else
        {
            lastHitPoint = null;
        }
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
