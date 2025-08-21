using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Tooltip("強化対象のツールのインデックス")]
    public List<int> targetToolIndices = new List<int>();

    [Tooltip("強化量")]
    public int upgradeAmount = 1;
}