using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("お宝のタグ")]
    public string treasureTag = "Treasure";

    public VRDigToolManager toolManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(treasureTag)) return;

        Debug.Log($"お宝 [{other.name}] を取得しました");

        // TreasureItem コンポーネントがあれば通常の強化
        TreasureItem item = other.GetComponent<TreasureItem>();
        if (item != null && toolManager != null)
        {
            foreach (int toolIndex in item.targetToolIndices)
            {
                toolManager.UpgradeTool(toolIndex, item.upgradeAmount);
            }
            toolManager.AddTreasureCount("Normal", 1);
            
            // 通知表示
            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("Normal");
            }
        }

        // HitZoneTreasureItem（判定数増加お宝）を処理
        HitZoneTreasureItem hitZoneItem = other.GetComponent<HitZoneTreasureItem>();
        if (hitZoneItem != null)
        {
            ProcessHitZoneTreasure(hitZoneItem);
            if (toolManager != null) toolManager.AddTreasureCount("PickaxeHitZone", 1);
            
            // 通知表示
            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("PickaxeHitZone");
            }
        }

        // DrillHitZoneTreasureItem（ドリル判定数増加お宝）を処理
        DrillHitZoneTreasureItem drillHitZoneItem = other.GetComponent<DrillHitZoneTreasureItem>();
        if (drillHitZoneItem != null)
        {
            ProcessDrillHitZoneTreasure(drillHitZoneItem);
            if (toolManager != null) toolManager.AddTreasureCount("DrillHitZone", 1);
            
            // 通知表示
            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("DrillHitZone");
            }
        }

        // DrillSpeedTreasureItem（ドリル速度増加お宝）を処理
        DrillSpeedTreasureItem drillSpeedItem = other.GetComponent<DrillSpeedTreasureItem>();
        if (drillSpeedItem != null)
        {
            ProcessDrillSpeedTreasure(drillSpeedItem);
            if (toolManager != null) toolManager.AddTreasureCount("DrillSpeed", 1);
            
            // 通知表示
            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("DrillSpeed");
            }
        }

        // ExplosivePickaxeTreasureItem（つるはし爆発お宝）を処理
        ExplosivePickaxeTreasureItem explosiveItem = other.GetComponent<ExplosivePickaxeTreasureItem>();
        if (explosiveItem != null)
        {
            ProcessExplosivePickaxeTreasure(explosiveItem);
            if (toolManager != null) toolManager.AddTreasureCount("Explosive", 1);
            
            // 通知表示
            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("Explosive");
            }
        }

        CompassMainUnlock compassMainUnlock = other.GetComponent<CompassMainUnlock>();
        if (compassMainUnlock != null)
        {
            
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.UnlockCompass();

            Debug.Log($"[{compassMainUnlock.treasureName}] コンパス本体を取得しました！");

            if (toolManager != null)
                toolManager.AddTreasureCount("CompassMain", 1);

            if (TreasureNotificationManager.Instance != null)
                TreasureNotificationManager.Instance.ShowTreasureNotification("CompassMain");
        }

        CompassAbilityTreasureItem compassUnlock = other.GetComponent<CompassAbilityTreasureItem>();
        if (compassUnlock != null)
        {
            ProcessCompassUnlock(compassUnlock);

            if (toolManager != null) toolManager.AddTreasureCount("CompassUnlock", 1);

            if (TreasureNotificationManager.Instance != null)
            {
                TreasureNotificationManager.Instance.ShowTreasureNotification("CompassUnlock");
            }
        }

        // お宝を非表示に
        Destroy(other.gameObject, 0.1f);
        //other.gameObject.SetActive(false);
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

    // DrillSpeedTreasureItemの処理
    private void ProcessDrillSpeedTreasure(DrillSpeedTreasureItem drillSpeedItem)
    {
        Debug.Log($"[{drillSpeedItem.treasureName}] ドリル速度増加お宝を処理中...");
        
        if (toolManager != null)
        {
            // VRDigToolManagerに速度増加を依頼（現在のツールに関係なく保存される）
            toolManager.IncreaseDrillSpeed(drillSpeedItem.speedIncreaseAmount);
            Debug.Log($"[{drillSpeedItem.treasureName}] ドリルの速度を {drillSpeedItem.speedIncreaseAmount} 段階アップさせました！");
        }
        else
        {
            Debug.LogWarning($"[{drillSpeedItem.treasureName}] ツールマネージャーが見つかりません。");
        }
    }

    // ExplosivePickaxeTreasureItemの処理
    private void ProcessExplosivePickaxeTreasure(ExplosivePickaxeTreasureItem explosiveItem)
    {
        Debug.Log($"[{explosiveItem.treasureName}] つるはし爆発お宝を処理中...");
        
        if (toolManager != null)
        {
            // VRDigToolManagerに爆発チャージ追加を依頼
            toolManager.AddPickaxeExplosionCharges(explosiveItem.chargesPerPickup);
            Debug.Log($"[{explosiveItem.treasureName}] つるはし爆発チャージを {explosiveItem.chargesPerPickup} 追加しました！");
        }
        else
        {
            Debug.LogWarning($"[{explosiveItem.treasureName}] ツールマネージャーが見つかりません。");
        }
    }

    //コンパスの機能開放用
    private void ProcessCompassUnlock(CompassAbilityTreasureItem item)
    {
        if (item.compassScript != null)
        {
            item.compassScript.searchCharenge = true;
            Debug.Log($"[{item.treasureName}] コンパス機能が解放されました！");
        }
        else
        {
            Debug.LogWarning($"[{item.treasureName}] CompassScript が設定されていません！");
        }
    }
}