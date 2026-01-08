using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassAbilityTreasureItem : MonoBehaviour
{
    [Tooltip("機能解放する対象のコンパススクリプト")]
    public CompassScript compassScript;

    [Tooltip("お宝の名前")]
    public string treasureName = "Compass Unlock Treasure";

    private void Start()
    {
        Debug.Log($"[{treasureName}] コンパス機能解放お宝が生成されました");
    }
}
