using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRDigToolSwitcher : MonoBehaviour
{
    public MonoBehaviour activeToolScript; // IDigToolを実装したスクリプトを指定（例：PickaxeDigTool）
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

    // 将来的に掘削ツールを切り替えるとき用に関数も用意
    public void SwitchTool(MonoBehaviour newToolScript)
    {
        activeToolScript = newToolScript;
        activeTool = newToolScript as IDigTool;
    }
}