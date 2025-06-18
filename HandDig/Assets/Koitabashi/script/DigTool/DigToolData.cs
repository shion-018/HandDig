using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DigToolData
{
    public string toolName;
    public DigToolStats stats;  // スクリプタブルオブジェクト
    [Min(0)] public int currentUpgradeLevel = 0;  // 現在の強化段階
}
