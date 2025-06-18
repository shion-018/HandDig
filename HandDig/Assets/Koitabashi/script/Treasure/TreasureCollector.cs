using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureCollector : MonoBehaviour
{
    [Tooltip("����Ƃ��ĔF�������^�O��")]
    public string treasureTag = "Treasure";

    public VRDigToolManager toolManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(treasureTag)) return;

        Debug.Log($"���� [{other.name}] �𔭌��I");

        // TreasureItem �R���|�[�l���g������΋���������
        TreasureItem item = other.GetComponent<TreasureItem>();
        if (item != null && toolManager != null)
        {
            foreach (int toolIndex in item.targetToolIndices)
            {
                toolManager.UpgradeTool(toolIndex, item.upgradeAmount);
            }
        }

        // ������\���Ɂi��ŃG�t�F�N�g�Ή��j
        other.gameObject.SetActive(false);
    }
}