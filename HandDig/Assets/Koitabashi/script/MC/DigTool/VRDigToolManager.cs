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
        // �@�폈���Ăяo��
        if (currentTool != null && currentToolTransform != null)
        {
            currentTool.UpdateDig(currentToolTransform.position);
        }

        // �c�[���؂�ւ��iQuest3�FB�{�^���j
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
            Debug.Log($"�c�[���؂�ւ�: {entry.toolScript.GetType().Name}");
        }
        else
        {
            Debug.LogError("�w�肳�ꂽ toolScript �� IDigTool ���������Ă��܂���B");
        }
    }
}