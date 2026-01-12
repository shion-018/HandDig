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
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    [Header("つるはし爆発モード設定")]
    [Tooltip("無限モード: チャージ関係なしに爆発モードを使用可能にする")]
    public bool infiniteExplosionMode = false;

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

    // ドリルの射出モード（お宝でアンロック）
    private bool drillShootModeUnlocked = false;

    // お宝取得数の追跡
    private int totalTreasureCount = 0;
    private int normalTreasureCount = 0;
    private int pickaxeHitZoneTreasureCount = 0;
    private int drillHitZoneTreasureCount = 0;
    private int drillSpeedTreasureCount = 0;
    private int explosiveTreasureCount = 0;
    private int drillShootModeTreasureCount = 0;

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

        // デバッグ: Uキーで全ての強化を一気に進める
        if (Input.GetKeyDown(KeyCode.U))
        {
            DebugUpgradeAll();
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
            var data = toolDataList[index];
            
            // 後方互換性のため残す
            if (data.stats != null)
            {
                if (digTool is VRDigTool handTool1)
                {
                    handTool1.SetStats(data.stats, data.currentUpgradeLevel);
                }
                else if (digTool is PickaxeDigToolMaster pickaxeMasterTool1)
                {
                    pickaxeMasterTool1.SetStats(data.stats, data.currentUpgradeLevel);
                }
                else if (digTool is DrillDigTool drillToolScript1)
                {
                    drillToolScript1.SetStats(data.stats, data.currentUpgradeLevel);
                }
            }
            
            // 各ツール専用ScriptableObjectを設定
            if (digTool is VRDigTool handTool && data.handStats != null)
            {
                handTool.SetHandStats(data.handStats, data.currentUpgradeLevel);
            }
            else if (digTool is PickaxeDigToolMaster pickaxeMasterTool && data.pickaxeStats != null)
            {
                pickaxeMasterTool.SetPickaxeStats(data.pickaxeStats, data.currentUpgradeLevel);
            }
            else if (digTool is DrillDigTool drillToolScript && data.drillStats != null)
            {
                drillToolScript.SetDrillStats(data.drillStats, data.currentUpgradeLevel);
            }

            // つるはしの場合、保存された判定数増加量を適用
            if (digTool is PickaxeDigToolMaster pickaxeMasterApply)
            {
                for (int i = 0; i < pickaxeHitZoneBonus; i++)
                {
                    pickaxeMasterApply.IncreaseHitZone();
                }
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] つるはしに切り替え: 判定数増加量 {pickaxeHitZoneBonus} を適用");
                
                // 適用後はリセット（重複適用を防ぐ）
                pickaxeHitZoneBonus = 0;
            }

            // ドリルの場合、保存された判定数増加量と速度増加量を適用
            if (digTool is DrillDigTool drillToolApply)
            {
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルに切り替え: 保存された判定数増加量 {drillHitZoneBonus} を適用開始");
                for (int i = 0; i < drillHitZoneBonus; i++)
                {
                    drillToolApply.IncreaseHitZone();
                }
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルに切り替え: 判定数増加量 {drillHitZoneBonus} を適用完了");
                
                // 速度増加量も適用
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルに切り替え: 保存された速度増加量 {drillSpeedBonus} を適用開始");
                for (int i = 0; i < drillSpeedBonus; i++)
                {
                    drillToolApply.IncreaseSpeed();
                }
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルに切り替え: 速度増加量 {drillSpeedBonus} を適用完了");
                
                // 射出モードが開放されている場合は適用
                if (drillShootModeUnlocked)
                {
                    drillToolApply.UnlockShootMode();
                }
                
                // 適用後はリセット（重複適用を防ぐ）
                drillHitZoneBonus = 0;
                drillSpeedBonus = 0;
                if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリル判定数・速度増加量をリセット: {drillHitZoneBonus}, {drillSpeedBonus}");
            }

            if (enableDebugLog) Debug.Log($"ツール切り替え: {entry.toolScript.GetType().Name}");
            TutorialManager.Instance?.OnToolChanged(currentTool, currentIndex);
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
                if (enableDebugLog) Debug.Log($"[強化] ツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
            else
            {
                if (enableDebugLog) Debug.Log($"[強化] ツール{index} はすでに最大強化されています。");
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
                if (enableDebugLog) Debug.Log($"[強化] Handツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }
        else if (data.pickaxeStats != null)
        {
            int max = data.pickaxeStats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                if (enableDebugLog) Debug.Log($"[強化] Pickaxeツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }
        else if (data.drillStats != null)
        {
            int max = data.drillStats.GetMaxUpgradeLevel();
            if (data.currentUpgradeLevel < max - 1)
            {
                int before = data.currentUpgradeLevel;
                data.currentUpgradeLevel = Mathf.Min(data.currentUpgradeLevel + amount, max - 1);
                if (enableDebugLog) Debug.Log($"[強化] Drillツール{index}を Lv.{before} から Lv.{data.currentUpgradeLevel} にアップ！");
            }
        }

        // 現在アクティブなツールに即座に適用
        if (index == currentIndex)
        {
            var currentData = toolDataList[index];
            
            // 後方互換性のため残す
            if (currentData.stats != null)
            {
                if (currentTool is VRDigTool handTool1)
                {
                    handTool1.SetStats(currentData.stats, currentData.currentUpgradeLevel);
                }
                else if (currentTool is PickaxeDigToolMaster pickaxeMasterTool1)
                {
                    pickaxeMasterTool1.SetStats(currentData.stats, currentData.currentUpgradeLevel);
                }
                else if (currentTool is DrillDigTool drillToolScript1)
                {
                    drillToolScript1.SetStats(currentData.stats, currentData.currentUpgradeLevel);
                }
            }
            
            // 各ツール専用ScriptableObjectを設定
            if (currentTool is VRDigTool handTool && currentData.handStats != null)
            {
                handTool.SetHandStats(currentData.handStats, currentData.currentUpgradeLevel);
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
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] つるはし使用中にお宝取得: 判定数を {amount} 増加（即座適用）");
        }
        else
        {
            // つるはしが非アクティブの場合のみ保存
            pickaxeHitZoneBonus += amount;
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] つるはしの判定数増加量を保存: {pickaxeHitZoneBonus}");
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
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリル使用中にお宝取得: 判定数を {amount} 増加（即座適用）");
        }
        else
        {
            // ドリルが非アクティブの場合のみ保存
            drillHitZoneBonus += amount;
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルの判定数増加量を保存: {drillHitZoneBonus}");
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
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリル使用中にお宝取得: 採掘速度を {amount} 段階アップ（即座適用）");
        }
        else
        {
            // ドリルが非アクティブの場合のみ保存
            drillSpeedBonus += amount;
            if (enableDebugLog) Debug.Log($"[VRDigToolManager] ドリルの速度増加量を保存: {drillSpeedBonus}");
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
        if (enableDebugLog) Debug.Log($"[VRDigToolManager] つるはし爆発チャージ +{add} => 残り {pickaxeExplosionCharges}");
    }

    public bool TryConsumePickaxeExplosionCharge()
    {
        // 無限モードの場合はチャージを消費せずに常にtrueを返す
        if (infiniteExplosionMode && pickaxeExplosionUnlocked)
        {
            return true;
        }
        
        if (!pickaxeExplosionUnlocked || pickaxeExplosionCharges <= 0) return false;
        pickaxeExplosionCharges--;
        return true;
    }

    // ---- Drill Shoot Mode (Treasure) APIs ----
    public void UnlockDrillShootMode()
    {
        drillShootModeUnlocked = true;
        // 現在ドリルがアクティブなら即座に適用
        if (currentTool is DrillDigTool drillToolUnlock)
        {
            drillToolUnlock.UnlockShootMode();
            if (enableDebugLog) Debug.Log("[VRDigToolManager] ドリル使用中にお宝取得: 射出モードを開放（即座適用）");
        }
        else
        {
            if (enableDebugLog) Debug.Log("[VRDigToolManager] ドリル射出モードを開放（次回ドリル使用時に適用）");
        }
    }

    public bool IsDrillShootModeUnlocked()
    {
        return drillShootModeUnlocked;
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
            case "DrillShootMode":
                drillShootModeTreasureCount += count;
                break;
        }
        
        if (enableDebugLog) Debug.Log($"[VRDigToolManager] お宝取得: {treasureType} +{count} (総数: {totalTreasureCount})");
    }

    public int GetTotalTreasureCount() => totalTreasureCount;
    public int GetNormalTreasureCount() => normalTreasureCount;
    public int GetPickaxeHitZoneTreasureCount() => pickaxeHitZoneTreasureCount;
    public int GetDrillHitZoneTreasureCount() => drillHitZoneTreasureCount;
    public int GetDrillSpeedTreasureCount() => drillSpeedTreasureCount;
    public int GetExplosiveTreasureCount() => explosiveTreasureCount;
    public int GetDrillShootModeTreasureCount() => drillShootModeTreasureCount;

    // ---- Debug Functions ----
    /// <summary>
    /// デバッグ用: 全ての強化を一気に最大まで進める（Uキーで呼び出される）
    /// </summary>
    private void DebugUpgradeAll()
    {
        Debug.Log("[VRDigToolManager] デバッグ: 全強化を実行します...");

        // 1. 全てのツールのレベルを最大まで上げる
        for (int i = 0; i < toolDataList.Count; i++)
        {
            var data = toolDataList[i];
            int maxLevel = 0;

            if (data.stats != null)
            {
                maxLevel = data.stats.GetMaxUpgradeLevel();
            }
            else if (data.handStats != null)
            {
                maxLevel = data.handStats.GetMaxUpgradeLevel();
            }
            else if (data.pickaxeStats != null)
            {
                maxLevel = data.pickaxeStats.GetMaxUpgradeLevel();
            }
            else if (data.drillStats != null)
            {
                maxLevel = data.drillStats.GetMaxUpgradeLevel();
            }

            if (maxLevel > 0)
            {
                int amount = maxLevel - 1 - data.currentUpgradeLevel;
                if (amount > 0)
                {
                    UpgradeTool(i, amount);
                }
            }
        }

        // 2. つるはしの判定数を最大まで上げる
        var pickaxeTool = tools.Find(entry => entry.toolScript is PickaxeDigToolMaster);
        if (pickaxeTool != null && pickaxeTool.toolScript is PickaxeDigToolMaster pickaxeMaster)
        {
            // digPointGroupsの数が最大レベル
            int maxHitZones = pickaxeMaster.digPointGroups != null ? pickaxeMaster.digPointGroups.Count : 1;
            int currentHitZones = 1; // 初期値（IncreaseHitZoneが呼ばれる回数をカウントする必要があるが、とりあえず多めに呼ぶ）
            for (int i = 0; i < maxHitZones * 2; i++) // 多めに呼んで確実に最大にする
            {
                IncreasePickaxeHitZone(1);
            }
            Debug.Log($"[VRDigToolManager] デバッグ: つるはしの判定数を最大まで上げました");
        }

        // 3. ドリルの判定数を最大まで上げる
        var drillTool = tools.Find(entry => entry.toolScript is DrillDigTool);
        if (drillTool != null && drillTool.toolScript is DrillDigTool drill)
        {
            int maxHitZones = drill.hitZones != null ? drill.hitZones.Count : 1;
            for (int i = 0; i < maxHitZones * 2; i++) // 多めに呼んで確実に最大にする
            {
                IncreaseDrillHitZone(1);
            }
            Debug.Log($"[VRDigToolManager] デバッグ: ドリルの判定数を最大まで上げました");
        }

        // 4. ドリルの速度を最大まで上げる
        var drillData = toolDataList.Find(data => data.drillStats != null);
        if (drillData != null && drillData.drillStats != null)
        {
            int maxSpeedLevel = drillData.drillStats.GetMaxSpeedUpgradeLevel();
            int currentSpeedLevel = drillData.currentSpeedUpgradeLevel;
            int amount = maxSpeedLevel - 1 - currentSpeedLevel;
            if (amount > 0)
            {
                IncreaseDrillSpeed(amount);
                Debug.Log($"[VRDigToolManager] デバッグ: ドリルの速度を最大まで上げました (Lv.{currentSpeedLevel} → Lv.{maxSpeedLevel - 1})");
            }
        }

        // 5. つるはしの爆発モードを開放してチャージを追加
        if (!pickaxeExplosionUnlocked)
        {
            UnlockPickaxeExplosion();
        }
        AddPickaxeExplosionCharges(100);
        Debug.Log("[VRDigToolManager] デバッグ: つるはしの爆発モードを開放し、チャージを100追加しました");

        // 6. ドリルの射出モードを開放
        if (!drillShootModeUnlocked)
        {
            UnlockDrillShootMode();
            Debug.Log("[VRDigToolManager] デバッグ: ドリルの射出モードを開放しました");
        }

        Debug.Log("[VRDigToolManager] デバッグ: 全強化完了！");
    }
}