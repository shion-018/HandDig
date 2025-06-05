using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;         // ワールド全体を参照
    public float digRadius = 2f;   // 掘る半径
    public void DigAt(Vector3 position)
    {
        DigAt(position, 1.0f); // デフォルトの半径で掘る（1.0fはお好みで）
    }
    public void DigAt(Vector3 position, float radius)
    {
        Debug.Log($"範囲{radius}で掘削！");

        if (world != null)
        {
            world.Dig(position, radius); // ← 実際にワールドから掘削する処理
        }
        else
        {
            Debug.LogError("MC_World がアタッチされていません！");
        }
    }
}