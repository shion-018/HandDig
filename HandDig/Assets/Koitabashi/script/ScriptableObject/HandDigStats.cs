using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/HandDigStats", fileName = "NewHandDigStats")]
public class HandDigStats : ScriptableObject
{
    [Header("掘削サイズ設定")]
    [Tooltip("初期掘削サイズ")]
    public float baseRadius = 1.0f;
    [Tooltip("強化最大の掘削サイズ")]
    public float maxRadius = 2.0f;
    [Tooltip("強化段階数")]
    [Min(1)] public int upgradeSteps = 3;

    [Header("採掘速度設定")]
    [Tooltip("初期採掘間隔（秒）")]
    public float baseDigInterval = 0.2f;
    [Tooltip("最大採掘間隔（秒）- 強化で加速した時の最小間隔")]
    public float maxDigInterval = 0.1f;
    [Tooltip("速度強化段階数")]
    [Min(1)] public int speedUpgradeSteps = 3;

    public float GetRadius(int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        return Mathf.Lerp(baseRadius, maxRadius, t);
    }

    public float GetDigInterval(int speedUpgradeLevel = 0)
    {
        float t = Mathf.Clamp01(speedUpgradeSteps == 1 ? 1f : (float)speedUpgradeLevel / (speedUpgradeSteps - 1));
        return Mathf.Lerp(baseDigInterval, maxDigInterval, t);
    }

    public int GetMaxUpgradeLevel() => upgradeSteps;
    public int GetMaxSpeedUpgradeLevel() => speedUpgradeSteps;
} 