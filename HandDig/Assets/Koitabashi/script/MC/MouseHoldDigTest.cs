using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHoldDigTest : MonoBehaviour
{
    public VoxelDigManager digManager;
    public LayerMask terrainLayer;
    public float digInterval = 0.1f; // Œ@‚éŠÔŠui•bj

    private float digTimer = 0f;
    private Vector3? lastHitPoint = null;

    void Update()
    {
        // Raycast‚ÅŒ@‚éêŠ‚ðŽæ“¾
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, terrainLayer))
        {
            lastHitPoint = hit.point;

            // ¶ƒNƒŠƒbƒN‰Ÿ‚µ‚Á‚Ï‚È‚µ‚Åˆê’èŠÔŠu‚²‚Æ‚ÉŒ@‚é
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
                digTimer = digInterval; // ‰Ÿ‚µ‚Ä‚È‚¢ŠÔ‚ÍŽŸ‰ñ‚·‚®Œ@‚ê‚é‚æ‚¤‚É‚µ‚Ä‚¨‚­
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
