using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

[System.Serializable]
public class DigToolEntry
{
    public GameObject toolObject;         // ���f����Collider�����I�u�W�F�N�g
    public MonoBehaviour toolScript;      // IDigTool ����������X�N���v�g
    public Transform toolTransform;       // �@��ʒu�i�ʏ�E���Transform�j
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

            // �e�c�[���Ƀ}�l�[�W���[��n���悤�ɂ���i������d�v�j
            if (digTool is IDigToolWithStats toolWithStats)
            {
                var data = toolDataList[index];
                toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
            }

            Debug.Log($"�c�[���؂�ւ�: {entry.toolScript.GetType().Name}");
        }
    }


    public void UpgradeTool(int index, int amount = 1)
    {
        if (index < 0 || index >= toolDataList.Count)
        {
            Debug.LogWarning($"�s���ȃc�[���C���f�b�N�X: {index}");
            return;
        }

        var data = toolDataList[index];
        int max = data.stats.GetMaxUpgradeLevel();

        if (data.currentUpgradeLevel < max - 1)
        {
            int before = data.currentUpgradeLevel;
            data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);

            Debug.Log($"[����] �c�[��{index}�� Lv.{before} �� Lv.{data.currentUpgradeLevel} �ɃA�b�v�I");

            if (index == currentIndex && currentTool is IDigToolWithStats toolWithStats)
            {
                toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
            }
        }
        else
        {
            Debug.Log($"[����] �c�[��{index} �͂��łɍő勭������Ă��܂��B");
        }
    }

}