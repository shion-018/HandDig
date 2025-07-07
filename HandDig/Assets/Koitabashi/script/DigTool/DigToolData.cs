using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DigToolData
{
    public string toolName;
    
    // 後方互換性のため残す
    public DigToolStats stats;
    
    // 各ツール専用ScriptableObject
    public HandDigStats handStats;
    public PickaxeDigStats pickaxeStats;
    public DrillDigStats drillStats;
    
    [Min(0)] public int currentUpgradeLevel = 0;  // 現在の強化段階
    [Min(0)] public int currentSpeedUpgradeLevel = 0;  // 現在の速度強化段階（ドリル用）
}
