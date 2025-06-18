using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigVolume : MonoBehaviour
{
    public float digValue = 0f;

    public void ApplyDig(MC_World world)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;

        // bounds ���� voxel ��Ԃ𑖍�
        for (float x = bounds.min.x; x <= bounds.max.x; x += 1f)
            for (float y = bounds.min.y; y <= bounds.max.y; y += 1f)
                for (float z = bounds.min.z; z <= bounds.max.z; z += 1f)
                {
                    Vector3 p = new Vector3(x, y, z);
                    if (col.bounds.Contains(p) && col.ClosestPoint(p) == p) // ���ɂ��邩
                    {
                        world.Dig(p, 0.5f, digValue); // �������������a�ōׂ����@��
                    }
                }

        Destroy(gameObject);
    }
}