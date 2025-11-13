using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillSpeedTreasureItem : MonoBehaviour
{
    [Tooltip("採掘速度を増やす量")]
    public int speedIncreaseAmount = 1;

    [Tooltip("お宝の名前（デバッグ用）")]
    public string treasureName = "Drill Speed Treasure";

    private void Start()
    {
        Debug.Log($"[{treasureName}] ドリル速度増加お宝が生成されました。増加量: {speedIncreaseAmount}");
    }
} 