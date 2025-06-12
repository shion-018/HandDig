using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DigToolEntry
{
    public GameObject toolObject;         // モデルやColliderを持つオブジェクト
    public MonoBehaviour toolScript;      // IDigTool を実装するスクリプト
    public Transform toolTransform;       // 掘る位置（通常右手のTransform）
}

public class VRDigToolManager : MonoBehaviour
{
    public List<DigToolEntry> tools = new List<DigToolEntry>();
    private int currentIndex = 0;

    private IDigTool currentTool;
    private Transform currentToolTransform;

    void Start()
    {
        if (tools.Count > 0)
        {
            ActivateTool(currentIndex);
        }
    }

    void Update()
    {
        // 掘削処理呼び出し
        if (currentTool != null && currentToolTransform != null)
        {
            currentTool.UpdateDig(currentToolTransform.position);
        }

        // ツール切り替え（Quest3：Bボタン）
        if (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.T))
        {
            CycleTool();
        }
    }

    void CycleTool()
    {
        currentIndex = (currentIndex + 1) % tools.Count;
        ActivateTool(currentIndex);
    }

    void ActivateTool(int index)
    {
        for (int i = 0; i < tools.Count; i++)
        {
            if (tools[i].toolObject != null)
            {
                tools[i].toolObject.SetActive(i == index);
            }
        }

        var entry = tools[index];
        if (entry.toolScript is IDigTool digTool)
        {
            currentTool = digTool;
            currentToolTransform = entry.toolTransform;
            Debug.Log($"ツール切り替え: {entry.toolScript.GetType().Name}");
        }
        else
        {
            Debug.LogError("指定された toolScript が IDigTool を実装していません。");
        }
    }
}