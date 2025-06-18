using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Tooltip("このお宝で強化されるツールのインデックス（複数可）")]
    public List<int> targetToolIndices = new List<int>(); // 複数選択

    [Tooltip("1回で強化する段階数")]
    public int upgradeAmount = 1;
}