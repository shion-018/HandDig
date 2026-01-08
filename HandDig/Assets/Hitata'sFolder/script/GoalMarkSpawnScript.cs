using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalMarkSpawnScript : MonoBehaviour
{
    [Header("スポーンさせるPrefab")]
    [SerializeField] private GameObject spawnPrefab;

    [Header("スポーン位置（Transform）")]
    [SerializeField] private Transform spawnPoint;

    [Header("生成時の回転をPrefabの回転にするか")]
    [SerializeField] private bool usePrefabRotation = true;

    public void Spawn()
    {
        if (spawnPrefab == null)
        {
            Debug.LogError("Spawn Prefab が設定されていません。");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point が設定されていません。");
            return;
        }

        Quaternion rotation = usePrefabRotation
            ? spawnPrefab.transform.rotation
            : spawnPoint.rotation;

        Instantiate(
            spawnPrefab,
            spawnPoint.position,
            rotation
        );
    }
}
