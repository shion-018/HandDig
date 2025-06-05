using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHoldDigTest : MonoBehaviour
{
    public VoxelDigManager digManager;
    public LayerMask terrainLayer;
    public float digInterval = 0.1f; // �@��Ԋu�i�b�j

    private float digTimer = 0f;
    private Vector3? lastHitPoint = null;

    void Update()
    {
        // Raycast�Ō@��ꏊ���擾
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, terrainLayer))
        {
            lastHitPoint = hit.point;

            // ���N���b�N�������ςȂ��ň��Ԋu���ƂɌ@��
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
                digTimer = digInterval; // �����ĂȂ��Ԃ͎��񂷂��@���悤�ɂ��Ă���
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
