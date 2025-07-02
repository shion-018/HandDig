using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    [Tooltip("配置するお宝プレハブの一覧（種類別）")]
    public List<GameObject> treasurePrefabs;

    [Tooltip("お宝を配置する確率（0～1）")]
    [Range(0f, 1f)]
    public float spawnChance = 0.1f;

    [Tooltip("1チャンクあたりの最大スポーン試行回数")]
    public int maxAttemptsPerChunk = 3;

    [Tooltip("1チャンクのサイズ（外部スクリプトから渡してもOK）")]
    public int chunkSize = 32;

    // 除外チャンク
    private HashSet<Vector3Int> excludedChunks = new HashSet<Vector3Int>();

    public void AddExcludedChunk(Vector3Int chunkCoord)
    {
        excludedChunks.Add(chunkCoord);
    }

    public void TrySpawnTreasureAtChunk(Vector3Int chunkCoord, Vector3 worldPos)
    {
        if (excludedChunks.Contains(chunkCoord)) return;

        for (int i = 0; i < maxAttemptsPerChunk; i++)
        {
            if (Random.value < spawnChance && treasurePrefabs.Count > 0)
            {
                Vector3 localOffset = new Vector3(
                    Random.Range(4f, chunkSize - 4f),
                    Random.Range(4f, chunkSize - 4f),
                    Random.Range(4f, chunkSize - 4f)
                );

                Vector3 spawnPos = worldPos + localOffset;

                GameObject prefab = treasurePrefabs[Random.Range(0, treasurePrefabs.Count)];
                Instantiate(prefab, spawnPos, Quaternion.identity);
                Debug.Log($"お宝（{prefab.name}）を生成: チャンク {chunkCoord} （試行 {i + 1}）");
            }
        }
    }
    public bool IsExcludedChunk(Vector3Int coord)
    {
        return excludedChunks.Contains(coord);
    }
}