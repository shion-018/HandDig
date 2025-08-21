using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;
    public float digRadius = 2f;
    public void DigAt(Vector3 position)
    {
        DigAt(position, 1.0f);
    }
    public void DigAt(Vector3 position, float radius)
    {

        if (world != null)
        {
            world.Dig(position, radius);
        }
        else
        {
            Debug.LogError("MC_World が設定されていません");
        }
    }
}