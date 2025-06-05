using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("お宝として認識されるタグ名")]
    public string treasureTag = "Treasure"; // インスペクターで設定可能

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(treasureTag))
        {
            Debug.Log($"お宝 [{other.name}] を発見！");

            // お宝を非表示に（後からエフェクトなど追加予定）
            other.gameObject.SetActive(false);
        }
    }
}
