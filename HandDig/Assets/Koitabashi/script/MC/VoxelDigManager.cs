using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;         // ���[���h�S�̂��Q��
    public float digRadius = 2f;   // �@�锼�a

    public void DigAt(Vector3 point)
    {
        world.Dig(point, digRadius);
    }
}