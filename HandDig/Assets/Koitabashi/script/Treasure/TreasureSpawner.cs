using System.Collections.Generic;
using UnityEngine;

public class TreasureSpawner : MonoBehaviour
{
    [Tooltip("お宝のプレハブのリスト")]
    public List<GameObject> treasurePrefabs;

    [Tooltip("お宝の出現確率（0~1）")]
    [Range(0f, 1f)]
    public float spawnChance = 0.1f;

    [Tooltip("1チャンクあたりの最小試行回数")]
    [Min(0)]
    public int minAttemptsPerChunk = 1;

    [Tooltip("1チャンクあたりの最大試行回数")]
    [Min(0)]
    public int maxAttemptsPerChunk = 5;

    [Tooltip("1チャンクのサイズ")]
    public int chunkSize = 32;

    [Header("距離による密度制御")]
    [Tooltip("スポーン密度が最大となる距離（プレイヤースポーン基準）")]
    public float nearDistance = 0f;

    [Tooltip("スポーン密度が最小になる距離")]
    public float farDistance = 150f;

    [Tooltip("距離正規化(0=近,1=遠)に対する補正カーブ")]
    public AnimationCurve distanceFalloff = AnimationCurve.EaseInOut(0f, 1.2f, 1f, 0.3f);

    [Header("スポーン候補の検証設定")]
    [Tooltip("チャンク端からのマージン")]
    public float chunkEdgePadding = 4f;

    [Tooltip("掘削済みエリアを避けるためのリトライ回数")]
    public int placementRetries = 6;

    [Tooltip("スポーン地点を固形地形とみなす密度閾値")]
    [Range(0f, 1f)]
    public float solidDensityThreshold = 0.55f;

    private SpawnManager spawnManager;

    public void InitializeContext(MC_World world, SpawnManager spawnManager)
    {
        this.spawnManager = spawnManager;
    }

    public void TrySpawnTreasureAtChunk(Vector3Int chunkCoord, MC_Chunk chunk)
    {
        if (chunk == null || treasurePrefabs == null || treasurePrefabs.Count == 0)
            return;

        Vector3 chunkCenter = chunk.transform.position + Vector3.one * chunk.chunkSize * 0.5f;
        float distanceWeight = EvaluateDistanceWeight(chunkCenter);
        if (distanceWeight <= 0f)
            return;

        int attempts = ResolveAttemptCount(distanceWeight);
        if (attempts <= 0)
            return;

        for (int i = 0; i < attempts; i++)
        {
            float weightedChance = spawnChance * distanceWeight;
            if (Random.value >= weightedChance)
                continue;

            if (!TryGetSpawnPosition(chunk, out Vector3 spawnPos))
                continue;

            GameObject prefab = treasurePrefabs[Random.Range(0, treasurePrefabs.Count)];
            Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            Debug.Log($"{prefab.name} が {chunkCoord} に生成されました (距離補正={distanceWeight:F2})");
        }
    }

    float EvaluateDistanceWeight(Vector3 chunkCenter)
    {
        if (spawnManager == null || spawnManager.GetSelectedSpawnPoint() == null)
            return 1f;

        Vector3 spawnPos = spawnManager.GetSpawnPosition();
        float distance = Vector3.Distance(chunkCenter, spawnPos);
        if (farDistance <= nearDistance)
            return Mathf.Max(0f, distanceFalloff.Evaluate(0f));

        float normalized = Mathf.InverseLerp(nearDistance, farDistance, distance);
        return Mathf.Max(0f, distanceFalloff.Evaluate(normalized));
    }

    int ResolveAttemptCount(float distanceWeight)
    {
        int minAttempts = Mathf.Max(0, minAttemptsPerChunk);
        int maxAttempts = Mathf.Max(minAttempts, maxAttemptsPerChunk);

        float lerp = Mathf.Clamp01(distanceWeight);
        float raw = Mathf.Lerp(minAttempts, maxAttempts, lerp);
        return Mathf.RoundToInt(raw);
    }

    bool TryGetSpawnPosition(MC_Chunk chunk, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (chunk.chunkData == null)
            return false;

        int retries = Mathf.Max(1, placementRetries);
        float padding = Mathf.Clamp(chunkEdgePadding, 0f, chunk.chunkSize * 0.5f);

        for (int attempt = 0; attempt < retries; attempt++)
        {
            Vector3 localOffset = new Vector3(
                Random.Range(padding, chunk.chunkSize - padding),
                Random.Range(padding, chunk.chunkSize - padding),
                Random.Range(padding, chunk.chunkSize - padding)
            );

            if (!IsSolidVoxel(chunk, localOffset))
                continue;

            worldPosition = chunk.transform.position + localOffset;
            return true;
        }

        return false;
    }

    bool IsSolidVoxel(MC_Chunk chunk, Vector3 localOffset)
    {
        var data = chunk.chunkData;
        int x = Mathf.Clamp(Mathf.RoundToInt(localOffset.x), 0, data.width);
        int y = Mathf.Clamp(Mathf.RoundToInt(localOffset.y), 0, data.height);
        int z = Mathf.Clamp(Mathf.RoundToInt(localOffset.z), 0, data.depth);
        return data.densityMap[x, y, z] >= solidDensityThreshold;
    }
}