using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolManager : MonoBehaviour
{
    public IDigTool currentTool;
    public Transform toolTransform; // 右手の位置を参照（＝ツールの位置）


    public MonoBehaviour defaultToolObject; // PickaxeDigTool をアタッチした GameObject
    public Transform defaultToolTransform;  // 右手の位置

    void Start()
    {
        if (defaultToolObject is IDigTool digTool)
        {
            SetTool(digTool, defaultToolTransform);
            Debug.Log("初期ツールをセットしたよ！");
        }
        else
        {
            Debug.LogError("defaultToolObject に IDigTool を実装したオブジェクトを設定してください！");
        }
    }
    void Update()
    {
        if (currentTool != null && toolTransform != null)
        {
            Debug.Log("VRDigToolManager が UpdateDig 呼んでるよ");
            currentTool.UpdateDig(toolTransform.position);
        }
        else
        {
            Debug.LogWarning("currentTool か toolTransform が null だよ！");
        }
        if(currentTool == null)
        {
            Debug.LogWarning("現在の掘削ツールが設定されていません。SetToolメソッドで設定してください。");
        }
    }
    public void SetTool(IDigTool tool, Transform toolTransform)
    {
        currentTool = tool;
        this.toolTransform = toolTransform;
    }
}