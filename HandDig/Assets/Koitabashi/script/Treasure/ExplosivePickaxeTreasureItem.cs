using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosivePickaxeTreasureItem : MonoBehaviour
{
    [Tooltip("1個取得時に付与する爆発チャージ数")]
    public int chargesPerPickup = 5;

    [Tooltip("お宝の名前（デバッグ用）")]
    public string treasureName = "Explosive Pickaxe Treasure";
}

