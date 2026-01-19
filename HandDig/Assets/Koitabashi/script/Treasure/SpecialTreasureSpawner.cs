using System.Collections.Generic;
using UnityEngine;

public class SpecialTreasureSpawner : MonoBehaviour
{
    [Header("特殊なお宝の設定")]
    [Tooltip("特殊なお宝のプレハブのリスト")]
    public List<GameObject> specialTreasurePrefabs = new List<GameObject>();

    [Header("スポーン位置の設定")]
    [Tooltip("お宝をスポーンする位置のリスト")]
    public List<Transform> spawnPositions = new List<Transform>();

    private void Start()
    {
        SpawnSpecialTreasures();
    }

    /// <summary>
    /// 特殊なお宝をスポーン位置にランダムで配置（被りなし）
    /// </summary>
    public void SpawnSpecialTreasures()
    {
        // プレハブとスポーン位置の数が一致しない場合は警告
        if (specialTreasurePrefabs == null || specialTreasurePrefabs.Count == 0)
        {
            Debug.LogWarning("[SpecialTreasureSpawner] お宝のプレハブが設定されていません。");
            return;
        }

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("[SpecialTreasureSpawner] スポーン位置が設定されていません。");
            return;
        }

        if (specialTreasurePrefabs.Count != spawnPositions.Count)
        {
            Debug.LogWarning($"[SpecialTreasureSpawner] プレハブの数({specialTreasurePrefabs.Count})とスポーン位置の数({spawnPositions.Count})が一致しません。");
            return;
        }

        // nullチェック
        for (int i = 0; i < specialTreasurePrefabs.Count; i++)
        {
            if (specialTreasurePrefabs[i] == null)
            {
                Debug.LogWarning($"[SpecialTreasureSpawner] プレハブリストの{i}番目がnullです。");
                return;
            }
        }

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (spawnPositions[i] == null)
            {
                Debug.LogWarning($"[SpecialTreasureSpawner] スポーン位置リストの{i}番目がnullです。");
                return;
            }
        }

        // プレハブのリストをコピーしてシャッフル
        List<GameObject> shuffledPrefabs = new List<GameObject>(specialTreasurePrefabs);
        ShuffleList(shuffledPrefabs);

        // 各スポーン位置に異なるお宝を配置
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            GameObject prefab = shuffledPrefabs[i];
            Transform spawnPos = spawnPositions[i];

            Instantiate(prefab, spawnPos.position, spawnPos.rotation, transform);
            Debug.Log($"[SpecialTreasureSpawner] {prefab.name} を {spawnPos.name} にスポーンしました。");
        }
    }

    /// <summary>
    /// リストをシャッフル（フィッシャー・イェーツのシャッフルアルゴリズム）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}


