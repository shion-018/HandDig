using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("����Ƃ��ĔF�������^�O��")]
    public string treasureTag = "Treasure";

    public VRDigToolManager toolManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(treasureTag)) return;

        Debug.Log($"���� [{other.name}] �𔭌��I");

        // TreasureItem コンポーネントがあれば通常の強化
        TreasureItem item = other.GetComponent<TreasureItem>();
        if (item != null && toolManager != null)
        {
            foreach (int toolIndex in item.targetToolIndices)
            {
                toolManager.UpgradeTool(toolIndex, item.upgradeAmount);
            }
        }

        // HitZoneTreasureItem（判定数増加お宝）を処理
        HitZoneTreasureItem hitZoneItem = other.GetComponent<HitZoneTreasureItem>();
        if (hitZoneItem != null)
        {
            ProcessHitZoneTreasure(hitZoneItem);
        }

        // DrillHitZoneTreasureItem（ドリル判定数増加お宝）を処理
        DrillHitZoneTreasureItem drillHitZoneItem = other.GetComponent<DrillHitZoneTreasureItem>();
        if (drillHitZoneItem != null)
        {
            ProcessDrillHitZoneTreasure(drillHitZoneItem);
        }

        // お宝を非表示に
        other.gameObject.SetActive(false);
    }

    // 判定数増加お宝の処理
    private void ProcessHitZoneTreasure(HitZoneTreasureItem hitZoneItem)
    {
        Debug.Log($"[{hitZoneItem.treasureName}] 判定数増加お宝を処理中...");
        
        if (toolManager != null)
        {
            // VRDigToolManagerに判定数増加を依頼（現在のツールに関係なく保存される）
            toolManager.IncreasePickaxeHitZone(hitZoneItem.hitZoneIncreaseAmount);
            Debug.Log($"[{hitZoneItem.treasureName}] つるはしの判定数を {hitZoneItem.hitZoneIncreaseAmount} 増加させました！");
        }
        else
        {
            Debug.LogWarning($"[{hitZoneItem.treasureName}] ツールマネージャーが見つかりません。");
        }
    }

    // DrillHitZoneTreasureItemの処理
    private void ProcessDrillHitZoneTreasure(DrillHitZoneTreasureItem drillHitZoneItem)
    {
        Debug.Log($"[{drillHitZoneItem.treasureName}] ドリル判定数増加お宝を処理中...");
        
        if (toolManager != null)
        {
            // VRDigToolManagerに判定数増加を依頼（現在のツールに関係なく保存される）
            toolManager.IncreaseDrillHitZone(drillHitZoneItem.hitZoneIncreaseAmount);
            Debug.Log($"[{drillHitZoneItem.treasureName}] ドリルの判定数を {drillHitZoneItem.hitZoneIncreaseAmount} 増加させました！");
        }
        else
        {
            Debug.LogWarning($"[{drillHitZoneItem.treasureName}] ツールマネージャーが見つかりません。");
        }
    }
}