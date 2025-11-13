using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DigToolEntry
{
    public GameObject toolObject;         // ツール本体のCollider付きオブジェクト
    public MonoBehaviour toolScript;      // IDigTool を実装したスクリプト
    public Transform toolTransform;       // ツール位置（通常はツールのTransform）
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
    
    // ドリルの速度レベル増加量を保存
    private int drillSpeedBonus = 0;

    // つるはしの爆発モード（お宝でアンロック）
    private int pickaxeExplosionCharges = 0;
    private bool pickaxeExplosionUnlocked = false;

    // お宝取得数の追跡
    private int totalTreasureCount = 0;
    private int normalTreasureCount = 0;
    private int pickaxeHitZoneTreasureCount = 0;
    private int drillHitZoneTreasureCount = 0;
    private int drillSpeedTreasureCount = 0;
    private int explosiveTreasureCount = 0;

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

            // 各ツールに専用ステータスを設定
            if (digTool is IDigToolWithStats toolWithStats)
            {
                var data = toolDataList[index];
                
                // 後方互換性のため残す
                if (data.stats != null)
                {
                    toolWithStats.SetStats(data.stats, data.currentUpgradeLevel);
                }
                
                // 各ツール専用ScriptableObjectを設定
                if (digTool is VRDigTool handTool && data.handStats != null)
                {
                    handTool.SetHandStats(data.handStats, data.currentUpgradeLevel);
                }
                else if (digTool is PickaxeDigTool pickaxeTool && data.pickaxeStats != null)
                {
                    pickaxeTool.SetPickaxeStats(data.pickaxeStats, data.currentUpgradeLevel);
                }
                else if (digTool is PickaxeDigToolMaster pickaxeMasterTool && data.pickaxeStats != null)
                {
                    pickaxeMasterTool.SetPickaxeStats(data.pickaxeStats, data.currentUpgradeLevel);
                }
                else if (digTool is DrillDigTool drillToolScript && data.drillStats != null)
                {
                    drillToolScript.SetDrillStats(data.drillStats, data.currentUpgradeLevel);
                }
            }

            // つるはしの場合、保存された判定数増加量を適用
            if (digTool is PickaxeDigToolMaster pickaxeMasterApply)
            {
                for (int i = 0; i < pickaxeHitZoneBonus; i++)
                {
                    pickaxeMasterApply.IncreaseHitZone();
                }
                Debug.Log($"[VRDigToolManager] つるはしに切り替え: 判定数増加量 {pickaxeHitZoneBonus} を適用");
                
                // 適用後はリセット（重複適用を防ぐ）
                pickaxeHitZoneBonus = 0;
            }

            // ドリルの場合、保存された判定数増加量と速度増加量を適用
            if (digTool is DrillDigTool drillToolApply)
            {
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 保存された判定数増加量 {drillHitZoneBonus} を適用開始");
                for (int i = 0; i < drillHitZoneBonus; i++)
                {
                    drillToolApply.IncreaseHitZone();
                }
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 判定数増加量 {drillHitZoneBonus} を適用完了");
                
                // 速度増加量も適用
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 保存された速度増加量 {drillSpeedBonus} を適用開始");
                for (int i = 0; i < drillSpeedBonus; i++)
                {
                    drillToolApply.IncreaseSpeed();
                }
                Debug.Log($"[VRDigToolManager] ドリルに切り替え: 速度増加量 {drillSpeedBonus} を適用完了");
                
                // 適用後はリセット（重複適用を防ぐ）
                drillHitZoneBonus = 0;
                drillSpeedBonus = 0;
                Debug.Log($"[VRDigToolManager] ドリル判定数・速度増加量をリセット: {drillHitZoneBonus}, {drillSpeedBonus}");
            }

            Debug.Log($"ツール切り替え: {entry.toolScript.GetType().Name}");
        }
    }

    public void UpgradeTool(int index, int amount = 1)
    {
        if (index < 0 || index >= toolDataList.Count)
        {
            Debug.LogWarning($"無効なツールインデックス: {index}");
            return;
        }

        var data = toolDataList[index];
        
        // 後方互換性のため残す
        if (data.stats != null)
        {
            int max = data.stats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                Debug.Log($"[強化] ツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
            else
            {
                Debug.Log($"[強化] ツール{index} はすでに最大強化されています。");
            }
        }
        
        // 各ツール専用ScriptableObjectの強化
        if (data.handStats != null)
        {
            int max = data.handStats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                Debug.Log($"[強化] Handツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }
        else if (data.pickaxeStats != null)
        {
            int max = data.pickaxeStats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                Debug.Log($"[強化] Pickaxeツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }
        else if (data.drillStats != null)
        {
            int max = data.drillStats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                Debug.Log($"[強化] Drillツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }

        // 現在アクティブなツールに即座に適用
        if (index == currentIndex && currentTool is IDigToolWithStats toolWithStats)
        {
            var currentData = toolDataList[index];
            
            // 後方互換性のため残す
            if (currentData.stats != null)
            {
                toolWithStats.SetStats(currentData.stats, currentData.currentUpgradeLevel);
            }
            
                         // 各ツール専用ScriptableObjectを設定
             if (currentTool is VRDigTool handTool && currentData.handStats != null)
             {
                 handTool.SetHandStats(currentData.handStats, currentData.currentUpgradeLevel);
             }
             else if (currentTool is PickaxeDigTool pickaxeTool && currentData.pickaxeStats != null)
             {
                 pickaxeTool.SetPickaxeStats(currentData.pickaxeStats, currentData.currentUpgradeLevel);
             }
             else if (currentTool is PickaxeDigToolMaster pickaxeMasterUpgrade && currentData.pickaxeStats != null)
             {
                 pickaxeMasterUpgrade.SetPickaxeStats(currentData.pickaxeStats, currentData.currentUpgradeLevel);
             }
             else if (currentTool is DrillDigTool drillToolUpgrade && currentData.drillStats != null)
             {
                 drillToolUpgrade.SetDrillStats(currentData.drillStats, currentData.currentUpgradeLevel);
             }
        }
    }

    // つるはしの判定数を増やす（お宝で呼び出される）
    public void IncreasePickaxeHitZone(int amount = 1)
    {
        // 現在つるはしがアクティブなら即座に適用（保存はしない）
        if (currentTool is PickaxeDigToolMaster pickaxeMasterBonus)
        {
            for (int i = 0; i < amount; i++)
            {
                pickaxeMasterBonus.IncreaseHitZone();
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
        if (currentTool is DrillDigTool drillToolBonus)
        {
            for (int i = 0; i < amount; i++)
            {
                drillToolBonus.IncreaseHitZone();
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

    // ドリルの採掘速度を加速（お宝で呼び出される）
    public void IncreaseDrillSpeed(int amount = 1)
    {
        // 現在ドリルがアクティブなら即座に適用（保存はしない）
        if (currentTool is DrillDigTool drillToolSpeed)
        {
            for (int i = 0; i < amount; i++)
            {
                drillToolSpeed.IncreaseSpeed();
            }
            Debug.Log($"[VRDigToolManager] ドリル使用中にお宝取得: 採掘速度を {amount} 段階アップ（即座適用）");
        }
        else
        {
            // ドリルが非アクティブの場合のみ保存
            drillSpeedBonus += amount;
            Debug.Log($"[VRDigToolManager] ドリルの速度増加量を保存: {drillSpeedBonus}");
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

    // ドリルの速度レベルを取得
    public int GetDrillSpeedLevel()
    {
        var drillData = toolDataList.Find(data => data.drillStats != null);
        if (drillData != null)
        {
            // 現在アクティブなドリルの場合は実際のレベルを返す
            if (currentTool is DrillDigTool)
            {
                return drillData.currentSpeedUpgradeLevel;
            }
            // 非アクティブの場合は保存されたボーナスも含める
            else
            {
                return drillData.currentSpeedUpgradeLevel + drillSpeedBonus;
            }
        }
        return 0;
    }

    // ドリルの最大速度レベルを取得
    public int GetDrillMaxSpeedLevel()
    {
        var drillData = toolDataList.Find(data => data.drillStats != null);
        if (drillData != null && drillData.drillStats != null)
        {
            return drillData.drillStats.GetMaxSpeedUpgradeLevel();
        }
        return 1;
    }
    
    /// <summary>
    /// 現在アクティブなツールを取得
    /// </summary>
    public IDigTool GetCurrentTool()
    {
        return currentTool;
    }
    
    /// <summary>
    /// 現在アクティブなツールのインデックスを取得
    /// </summary>
    public int GetCurrentToolIndex()
    {
        return currentIndex;
    }

    // ---- Pickaxe Explosion (Treasure) APIs ----
    public void UnlockPickaxeExplosion()
    {
        pickaxeExplosionUnlocked = true;
    }

    public bool IsPickaxeExplosionUnlocked()
    {
        return pickaxeExplosionUnlocked;
    }

    public int GetPickaxeExplosionCharges()
    {
        return pickaxeExplosionCharges;
    }

    public void AddPickaxeExplosionCharges(int add)
    {
        pickaxeExplosionUnlocked = true;
        pickaxeExplosionCharges = Mathf.Max(0, pickaxeExplosionCharges + add);
        Debug.Log($"[VRDigToolManager] つるはし爆発チャージ +{add} => 残り {pickaxeExplosionCharges}");
    }

    public bool TryConsumePickaxeExplosionCharge()
    {
        if (!pickaxeExplosionUnlocked || pickaxeExplosionCharges <= 0) return false;
        pickaxeExplosionCharges--;
        return true;
    }

    // ---- Treasure Count APIs ----
    public void AddTreasureCount(string treasureType, int count = 1)
    {
        totalTreasureCount += count;
        
        switch (treasureType)
        {
            case "Normal":
                normalTreasureCount += count;
                break;
            case "PickaxeHitZone":
                pickaxeHitZoneTreasureCount += count;
                break;
            case "DrillHitZone":
                drillHitZoneTreasureCount += count;
                break;
            case "DrillSpeed":
                drillSpeedTreasureCount += count;
                break;
            case "Explosive":
                explosiveTreasureCount += count;
                break;
        }
        
        Debug.Log($"[VRDigToolManager] お宝取得: {treasureType} +{count} (総数: {totalTreasureCount})");
    }

    public int GetTotalTreasureCount() => totalTreasureCount;
    public int GetNormalTreasureCount() => normalTreasureCount;
    public int GetPickaxeHitZoneTreasureCount() => pickaxeHitZoneTreasureCount;
    public int GetDrillHitZoneTreasureCount() => drillHitZoneTreasureCount;
    public int GetDrillSpeedTreasureCount() => drillSpeedTreasureCount;
    public int GetExplosiveTreasureCount() => explosiveTreasureCount;
}