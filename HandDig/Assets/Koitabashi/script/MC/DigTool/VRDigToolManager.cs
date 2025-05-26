using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolManager : MonoBehaviour
{
    public IDigTool currentTool;
    public Transform toolTransform; // 右手の位置を参照（＝ツールの位置）

    void Update()
    {
        if (currentTool != null)
        {
            currentTool.UpdateDig(toolTransform.position);
        }
    }

    public void SetTool(IDigTool newTool)
    {
        currentTool = newTool;
    }
}