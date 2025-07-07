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

    public float GetRadius(int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        return Mathf.Lerp(baseRadius, maxRadius, t);
    }

    public int GetMaxUpgradeLevel() => upgradeSteps;
} 