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

    /// <summary>
    /// 掘削を試行し、実際に掘削が発生したかどうかを返す
    /// </summary>
    /// <param name="position">掘削位置</param>
    /// <param name="radius">掘削半径</param>
    /// <returns>実際に掘削が発生した場合true</returns>
    public bool TryDigAt(Vector3 position, float radius)
    {
        MC_World target = worldRouter != null && worldRouter.CurrentWorld != null ? worldRouter.CurrentWorld : world;
        if (target != null)
        {
            return target.TryDig(position, radius);
        }
        else
        {
            Debug.LogError("MC_World が設定されていません");
            return false;
        }
    }
}