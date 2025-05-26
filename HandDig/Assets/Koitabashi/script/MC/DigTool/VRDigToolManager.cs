using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolManager : MonoBehaviour
{
    public IDigTool currentTool;
    public Transform toolTransform; // �E��̈ʒu���Q�Ɓi���c�[���̈ʒu�j

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