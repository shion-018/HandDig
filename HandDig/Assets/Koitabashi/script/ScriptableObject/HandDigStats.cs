using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/HandDigStats", fileName = "NewHandDigStats")]
public class HandDigStats : ScriptableObject
{
    [Header("ハンド 掘削サイズ設定")]
    [Tooltip("初期掘削サイズ")]
    public float baseRadius = 1.0f;
    [Tooltip("強化最大の掘削サイズ")]
    public float maxRadius = 2.0f;
    [Tooltip("強化段階数")]
    [Min(1)] public int upgradeSteps = 3;

    [Header("ハンド 掘削速度設定")]
    [Tooltip("初期掘削間隔（秒）")]
    public float baseDigInterval = 0.2f;
    [Tooltip("最大掘削間隔（秒）- お宝取得で加速した時の最小間隔")]
    public float maxDigInterval = 0.1f;
    [Tooltip("お宝取得時の加速段階数")]
    [Min(1)] public int speedUpgradeSteps = 3;

    // 掘削サイズを取得（強化レベルで補間）
    public float GetRadius(int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        return Mathf.Lerp(baseRadius, maxRadius, t);
    }

    // 現在の掘削間隔を取得（スピードアップレベルで補間）
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