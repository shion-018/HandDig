using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDigManager : MonoBehaviour
{
    public MC_World world;
    [Tooltip("現在の掘削先ワールドを管理するルーター（設定されていれば優先）")]
    public CurrentWorldRouter worldRouter;
    public float digRadius = 2f;
    public void DigAt(Vector3 position)
    {
        DigAt(position, 1.0f);
    }
    public void DigAt(Vector3 position, float radius)
    {

        MC_World target = worldRouter != null && worldRouter.CurrentWorld != null ? worldRouter.CurrentWorld : world;
        if (target != null)
        {
            target.Dig(position, radius);
        }
        else
        {
            Debug.LogError("MC_World が設定されていません");
        }
    }
}