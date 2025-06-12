using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dig/DigToolStats", fileName = "NewDigToolStats")]
public class DigToolStats : ScriptableObject
{
    public float baseRadius = 1.5f;
    public float stage2Radius = 2.0f;
    public float stage3Radius = 2.5f;
    public float upgradeBonus = 0f;

    public float GetRadius(int comboStage = 0)
    {
        float radius = comboStage switch
        {
            1 => stage2Radius,
            2 => stage3Radius,
            _ => baseRadius
        };
        return radius + upgradeBonus;
    }
}
