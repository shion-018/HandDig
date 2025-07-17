using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/DrillDigStats", fileName = "NewDrillDigStats")]
public class DrillDigStats : ScriptableObject
{
    [Header("掘削サイズ設定")]
    [Tooltip("初期掘削サイズ")]
    public float baseRadius = 1.5f;
    [Tooltip("強化最大の掘削サイズ")]
    public float maxRadius = 3f;
    [Tooltip("強化段階数")]
    [Min(1)] public int upgradeSteps = 5;

    [Header("採掘速度設定")]
    [Tooltip("初期採掘間隔（秒）")]
    public float baseDigInterval = 0.1f;
    [Tooltip("最大採掘間隔（秒）- お宝取得で加速した時の最小間隔")]
    public float maxDigInterval = 0.05f;
    [Tooltip("お宝取得時の加速段階数")]
    [Min(1)] public int speedUpgradeSteps = 5;

    // 掘削サイズを取得（強化レベルで補間）
    public float GetRadius(int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        return Mathf.Lerp(baseRadius, maxRadius, t);
    }

    // 現在の採掘間隔を取得（スピードアップレベルで補間）
    public float GetDigInterval(int speedUpgradeLevel = 0)
    {
        float t = Mathf.Clamp01(speedUpgradeSteps == 1 ? 1f : (float)speedUpgradeLevel / (speedUpgradeSteps - 1));
        return Mathf.Lerp(baseDigInterval, maxDigInterval, t);
    }

    public int GetMaxUpgradeLevel()
    {
        return upgradeSteps;
    }

    public int GetMaxSpeedUpgradeLevel()
    {
        return speedUpgradeSteps;
    }
} 