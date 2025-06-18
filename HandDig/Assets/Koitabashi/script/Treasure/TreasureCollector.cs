using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("お宝として認識されるタグ名")]
    public string treasureTag = "Treasure";

    public VRDigToolManager toolManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(treasureTag)) return;

        Debug.Log($"お宝 [{other.name}] を発見！");

        // TreasureItem コンポーネントがあれば強化処理へ
        TreasureItem item = other.GetComponent<TreasureItem>();
        if (item != null && toolManager != null)
        {
            foreach (int toolIndex in item.targetToolIndices)
            {
                toolManager.UpgradeTool(toolIndex, item.upgradeAmount);
            }
        }

        // お宝を非表示に（後でエフェクト対応可）
        other.gameObject.SetActive(false);
    }
}