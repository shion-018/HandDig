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

    // つるはしの判定数増加量を保存
    private int pickaxeHitZoneBonus = 0;
    
    // ドリルの判定数増加量を保存
    private int drillHitZoneBonus = 0;

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

            // 各ツールにステータスを設定するようにする（重要）
            if (digTool is IDigToolWithStats toolWithStats)
            {
                var data = toolDataList[index];
                toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
            }

            // つるはしの場合、保存された判定数増加量を適用
            if (digTool is PickaxeDigToolMaster pickaxeMaster)
            {
                for (int i = 0; i < pickaxeHitZoneBonus; i++)
                {
                    pickaxeMaster.IncreaseHitZone();
                }
                Debug.Log($"[VRDigToolManager] つるはしに切り替え: 判定数増加量 {pickaxeHitZoneBonus} を適用");
                
                // 適用後はリセット（重複適用を防ぐ）
                pickaxeHitZoneBonus = 0;
            }

            // ドリルの場合、保存された判定数増加量を適用
            if (digTool is DrillDigTool drillTool)
            {
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 保存された判定数増加量 {drillHitZoneBonus} を適用開始");
                for (int i = 0; i < drillHitZoneBonus; i++)
                {
                    drillTool.IncreaseHitZone();
                }
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 判定数増加量 {drillHitZoneBonus} を適用完了");
                
                // 適用後はリセット（重複適用を防ぐ）
                drillHitZoneBonus = 0;
                Debug.Log($"[VRDigToolManager] ドリル判定数増加量をリセット: {drillHitZoneBonus}");
            }

            Debug.Log($"ツール切り替え: {entry.toolScript.GetType().Name}");
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

    public int GetCurrentToolIndex()
    {
        return currentIndex;
    }

    // つるはしの判定数を増やす（お宝で呼び出される）
    public void IncreasePickaxeHitZone(int amount = 1)
    {
        // 現在つるはしがアクティブなら即座に適用（保存はしない）
        if (currentTool is PickaxeDigToolMaster pickaxeMaster)
        {
            for (int i = 0; i < amount; i++)
            {
                pickaxeMaster.IncreaseHitZone();
            }
            Debug.Log($"[VRDigToolManager] つるはし使用中にお宝取得: 判定数を {amount} 増加（即座適用）");
        }
        else
        {
            // つるはしが非アクティブの場合のみ保存
            pickaxeHitZoneBonus += amount;
            Debug.Log($"[VRDigToolManager] つるはしの判定数増加量を保存: {pickaxeHitZoneBonus}");
        }
    }

    // ドリルの判定数を増やす（お宝で呼び出される）
    public void IncreaseDrillHitZone(int amount = 1)
    {
        // 現在ドリルがアクティブなら即座に適用（保存はしない）
        if (currentTool is DrillDigTool drillTool)
        {
            for (int i = 0; i < amount; i++)
            {
                drillTool.IncreaseHitZone();
            }
            Debug.Log($"[VRDigToolManager] ドリル使用中にお宝取得: 判定数を {amount} 増加（即座適用）");
        }
        else
        {
            // ドリルが非アクティブの場合のみ保存
            drillHitZoneBonus += amount;
            Debug.Log($"[VRDigToolManager] ドリルの判定数増加量を保存: {drillHitZoneBonus}");
        }
    }

    // つるはしの判定数増加量を取得
    public int GetPickaxeHitZoneBonus()
    {
        return pickaxeHitZoneBonus;
    }

    // ドリルの判定数増加量を取得
    public int GetDrillHitZoneBonus()
    {
        return drillHitZoneBonus;
    }
}