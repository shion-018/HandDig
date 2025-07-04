using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/DigToolStatsAdvanced", fileName = "NewDigToolStatsAdvanced")]
public class DigToolStats : ScriptableObject
{
    [Header("�� �ʏ�̌@��T�C�Y�i�����l�j")]
    public float baseRadius = 1.5f;
    public float stage2Radius = 2.0f;
    public float stage3Radius = 2.5f;

    [Header("�� �����ő厞�̌@��T�C�Y")]
    public float baseRadiusMax = 2.5f;
    public float stage2RadiusMax = 3.0f;
    public float stage3RadiusMax = 3.5f;

    [Header("�� �����i�K�i1�ȏ�j")]
    [Min(1)] public int upgradeSteps = 3;

    [Header("採掘間隔設定")]
    [Tooltip("初期採掘間隔（秒）")]
    public float baseDigInterval = 0.1f;
    [Tooltip("最大採掘間隔（秒）- お宝取得で加速した時の最小間隔")]
    public float maxDigInterval = 0.05f;
    [Tooltip("お宝取得時の加速段階数")]
    [Min(1)] public int speedUpgradeSteps = 5;

    //@TCY擾�i�������x�����w��j
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
    public int GetMaxUpgradeLevel()
    {
        return upgradeSteps;
    }
}