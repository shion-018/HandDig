using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitZoneTreasureItem : MonoBehaviour
{
    [Tooltip("判定数を増やす量")]
    public int hitZoneIncreaseAmount = 1;

    [Tooltip("お宝の名前（デバッグ用）")]
    public string treasureName = "HitZone Treasure";

    private void Start()
    {
        Debug.Log($"[{treasureName}] 判定数増加お宝が生成されました。増加量: {hitZoneIncreaseAmount}");
    }
} 