using System.Collections.Generic;
using UnityEngine;

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
}