using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

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
    public List<DigToolData> toolDataList = new List<DigToolData>();

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
        if (currentTool != null && currentToolTransform != null)
        {
            currentTool.UpdateDig(currentToolTransform.position);
        }

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
                tools[i].toolObject.SetActive(i == index);
        }

        var entry = tools[index];
        if (entry.toolScript is IDigTool digTool)
        {
            currentTool = digTool;
            currentToolTransform = entry.toolTransform;

            // 各ツールにマネージャーを渡すようにする（←これ重要）
            if (digTool is IDigToolWithStats toolWithStats)
            {
                var data = toolDataList[index];
                toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
            }

            Debug.Log($"ツール切り替え: {entry.toolScript.GetType().Name}");
        }
    }


    public void UpgradeTool(int index, int amount = 1)
    {
        if (index < 0 || index >= toolDataList.Count)
        {
            Debug.LogWarning($"不正なツールインデックス: {index}");
            return;
        }

        var data = toolDataList[index];
        int max = data.stats.GetMaxUpgradeLevel();

        if (data.currentUpgradeLevel < max - 1)
        {
            int before = data.currentUpgradeLevel;
            data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);

            Debug.Log($"[強化] ツール{index}が Lv.{before} → Lv.{data.currentUpgradeLevel} にアップ！");

            if (index == currentIndex && currentTool is IDigToolWithStats toolWithStats)
            {
                toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
            }
        }
        else
        {
            Debug.Log($"[強化] ツール{index} はすでに最大強化されています。");
        }
    }

}