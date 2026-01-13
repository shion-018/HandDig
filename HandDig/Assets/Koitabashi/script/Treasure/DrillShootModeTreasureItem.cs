using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillShootModeTreasureItem : MonoBehaviour
{
    [Tooltip("お宝の名前（デバッグ用）")]
    public string treasureName = "Drill Shoot Mode Treasure";

    private void Start()
    {
        Debug.Log($"[{treasureName}] ドリル射出モード開放お宝が生成されました。");
    }
}

