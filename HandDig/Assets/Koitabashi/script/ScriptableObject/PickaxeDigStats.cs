using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/PickaxeDigStats", fileName = "NewPickaxeDigStats")]
public class PickaxeDigStats : ScriptableObject
{
    [Header("掘削サイズ設定（コンボ段階対応）")]
    [Tooltip("通常掘削サイズ")]
    public float baseRadius = 1.2f;
    public float stage2Radius = 1.7f;
    public float stage3Radius = 2.2f;
    [Tooltip("強化最大の掘削サイズ")]
    public float baseRadiusMax = 2.2f;
    public float stage2RadiusMax = 2.7f;
    public float stage3RadiusMax = 3.2f;
    [Tooltip("強化段階数")]
    [Min(1)] public int upgradeSteps = 5;

    [Header("爆発モード設定")]
    [Tooltip("爆発モードを使用可能にするか")]
    public bool enableExplosionMode = true;

    [Tooltip("爆発までの遅延秒数")]
    public float explosionDelaySeconds = 5f;

    [Tooltip("爆発半径（最小～最大、強化段階により補間）")]
    public float explosionRadiusBase = 3.5f; // 通常の最大サイズより大きめ
    public float explosionRadiusMax = 5.0f;

    [Tooltip("爆発マーカーのプレハブ（ExplosiveMarker を付与）")]
    public GameObject explosionMarkerPrefab;

    public float GetRadius(int comboStage = 0, int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        float min = comboStage switch
        {
            1 => stage2Radius,
            2 => stage3Radius,
            _ => baseRadius
        };
        float max = comboStage switch
        {
            1 => stage2RadiusMax,
            2 => stage3RadiusMax,
            _ => baseRadiusMax
        };
        return Mathf.Lerp(min, max, t);
    }

    public float GetExplosionRadius(int upgradeLevel = 0)
    {
        float t = Mathf.Clamp01(upgradeSteps == 1 ? 1f : (float)upgradeLevel / (upgradeSteps - 1));
        return Mathf.Lerp(explosionRadiusBase, explosionRadiusMax, t);
    }

    public int GetMaxUpgradeLevel() => upgradeSteps;
} 