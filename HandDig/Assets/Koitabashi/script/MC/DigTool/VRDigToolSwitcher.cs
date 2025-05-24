using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolSwitcher : MonoBehaviour
{
    public MonoBehaviour activeToolScript; // IDigTool�����������X�N���v�g���w��i��FPickaxeDigTool�j
    private IDigTool activeTool;

    void Start()
    {
        activeTool = activeToolScript as IDigTool;
    }

    void OnTriggerEnter(Collider other)
    {
        activeTool?.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        activeTool?.OnTriggerExit(other);
    }

    void Update()
    {
        activeTool?.UpdateDig(transform.position);
    }

    // �����I�Ɍ@��c�[����؂�ւ���Ƃ��p�Ɋ֐����p��
    public void SwitchTool(MonoBehaviour newToolScript)
    {
        activeToolScript = newToolScript;
        activeTool = newToolScript as IDigTool;
    }
}