using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillHitZoneTreasureItem : MonoBehaviour
{
    [Tooltip("判定数を増やす量")]
    public int hitZoneIncreaseAmount = 1;

    [Tooltip("お宝の名前（デバッグ用）")]
    public string treasureName = "Drill HitZone Treasure";

    private void Start()
    {
        Debug.Log($"[{treasureName}] ドリル判定数増加お宝が生成されました。増加量: {hitZoneIncreaseAmount}");
    }
} 