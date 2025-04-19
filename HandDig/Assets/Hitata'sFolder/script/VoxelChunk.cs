using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunk : MonoBehaviour
{

    public int size;
    public float[,,] density; // -1.0 (��) �` 1.0 (�n�`)

    public VoxelChunk(int size)
    {
        this.size = size;
        density = new float[size + 1, size + 1, size + 1];

        for (int x = 0; x <= size; x++)
            for (int y = 0; y <= size; y++)
                for (int z = 0; z <= size; z++)
                {
                    density[x, y, z] = 1f; // ������Ԃ�Solid�i�@���Ă��Ȃ��j
                }
    }

    public void Dig(Vector3 localPos, float radius)
    {
        for (int x = 0; x <= size; x++)
            for (int y = 0; y <= size; y++)
                for (int z = 0; z <= size; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    if ((pos - localPos).magnitude < radius)
                    {
                        density[x, y, z] = -1f; // ��ɂ���
                    }
                }
    }
}
