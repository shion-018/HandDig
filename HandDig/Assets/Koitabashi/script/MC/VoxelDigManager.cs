using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;         // ワールド全体を参照
    public float digRadius = 2f;   // 掘る半径

    public void DigAt(Vector3 point)
    {
        world.Dig(point, digRadius);
    }
}