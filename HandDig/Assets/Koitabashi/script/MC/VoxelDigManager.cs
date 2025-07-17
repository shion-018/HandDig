using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;         // ���[���h�S�̂��Q��
    public float digRadius = 2f;   // �@�锼�a
    public void DigAt(Vector3 position)
    {
        DigAt(position, 1.0f); // �f�t�H���g�̔��a�Ō@��i1.0f�͂��D�݂Łj
    }
    public void DigAt(Vector3 position, float radius)
    {
        Debug.Log($"�͈�{radius}�Ō@��I");

        if (world != null)
        {
            world.Dig(position, radius); // �� ���ۂɃ��[���h����@�킷�鏈��
        }
        else
        {
            Debug.LogError("MC_World ���A�^�b�`����Ă��܂���I");
        }
    }
}