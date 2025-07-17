using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MC_World : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 32;
    public int chunkCountX = 1;
    public int chunkCountY = 5;
    public int chunkCountZ = 1;

    [Header("初期化方式切り替え")]
    [Tooltip("true: 分散処理 (非同期)、false: 一気に同期処理")]
    public bool useAsyncInitialization = true;

    [Tooltip("お宝を生成するTreasureSpawnerのリスト")]
    public List<TreasureSpawner> treasureSpawners = new List<TreasureSpawner>();

    public List<DigVolume> digVolumesToApply;

    Dictionary<Vector3Int, MC_Chunk> chunkMap = new Dictionary<Vector3Int, MC_Chunk>();

    void Start()
    {
        Debug.Log("[MC_World] ワールド初期化開始");
        // 非同期初期化のみ実行
        InitializeWorldAsync().Forget();
    }

    async UniTask InitializeWorldAsync()
    {
        // 各TreasureSpawnerにchunkSizeを適用
        foreach (var spawner in treasureSpawners)
        {
            if (spawner == null) continue;
            spawner.chunkSize = chunkSize;
        }

        // チャンク生成を非同期で実行
        await GenerateChunksAsync();

        // DigVolumeの処理を非同期で実行
        await ApplyDigVolumesAsync();

        // スポーンシステムの処理
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager == null)
        {
            // 従来の固定スポーン処理（後方互換性のため残す）
            Vector3 startDigPos = new Vector3(
                chunkSize * chunkCountX / 2f,
                -chunkSize * 2,
                chunkSize * chunkCountZ / 2f
            );
            Dig(startDigPos, 10f);
            
            Debug.Log("[MC_World] SpawnManagerが見つかりません。固定スポーンを使用します。");
        }
        else
        {
            Debug.Log("[MC_World] SpawnManagerが設定されています。ランダムスポーンシステムを使用します。");
        }
        
        // プレイヤーを実際のスポーン位置に移動
        SpawnPlayerAtFinalPosition();
        
        Debug.Log("[MC_World] ワールド初期化完了");
    }

    async UniTask GenerateChunksAsync()
    {
        Debug.Log("[MC_World] チャンク生成開始");
        
        for (int x = 0; x < chunkCountX; x++)
        {
            for (int y = 0; y < chunkCountY; y++)
            {
                for (int z = 0; z < chunkCountZ; z++)
                {
                    int shiftedY = -y;

                    Vector3Int pos = new Vector3Int(x, shiftedY, z);
                    Vector3 worldPos = new Vector3(
                        x * chunkSize,
                        shiftedY * chunkSize,
                        z * chunkSize
                    );

                    GameObject obj = Instantiate(chunkPrefab, worldPos, Quaternion.identity, transform);
                    MC_Chunk chunk = obj.GetComponent<MC_Chunk>();
                    chunk.Initialize(worldPos);
                    chunkMap[pos] = chunk;

                    // 除外チャンクの判定
                    bool isExcluded = false;
                    foreach (var spawner in treasureSpawners)
                    {
                        if (spawner != null && spawner.IsExcludedChunk(pos))
                        {
                            isExcluded = true;
                            break;
                        }
                    }
                    chunk.isExcluded = isExcluded;
                    chunkMap[pos] = chunk;
                    
                    // お宝の生成
                    foreach (var spawner in treasureSpawners)
                    {
                        if (spawner != null)
                            spawner.TrySpawnTreasureAtChunk(pos, worldPos);
                    }
                    
                    // 分散処理する場合のみawait
                    if (useAsyncInitialization && (x + y + z) % 2 == 0)
                    {
                        await Cysharp.Threading.Tasks.UniTask.Yield();
                    }
                }
            }
        }
        
        Debug.Log("[MC_World] チャンク生成完了");
    }

    async UniTask ApplyDigVolumesAsync()
    {
        Debug.Log("[MC_World] DigVolume処理開始");

        // スポーンポイントのDigVolumeを最初に処理
        DigVolume spawnDigVolume = null;
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null && spawnManager.GetSelectedSpawnPoint() != null)
        {
            var spawnMarker = spawnManager.GetSelectedSpawnPoint();
            spawnDigVolume = spawnMarker.GetComponent<DigVolume>();
        }

        if (spawnDigVolume != null)
        {
            if (useAsyncInitialization)
                await spawnDigVolume.ApplyDigAsync(this);
            else
                spawnDigVolume.ApplyDigSync(this);
        }

        // 残りのDigVolumeを順番に処理（スポーンポイントのものは除外）
        foreach (var vol in digVolumesToApply)
        {
            if (vol != null && vol != spawnDigVolume)
            {
                if (useAsyncInitialization)
                    await vol.ApplyDigAsync(this);
                else
                    vol.ApplyDigSync(this);
            }
        }

        Debug.Log("[MC_World] DigVolume処理完了");
    }

    void SpawnPlayerAtFinalPosition()
    {
        // SpawnManagerがある場合はそちらでスポーン
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.SpawnPlayerAtFinalPosition();
        }
        else
        {
            // 固定スポーン位置に移動
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 spawnPos = new Vector3(
                    chunkSize * chunkCountX / 2f,
                    10f, // 少し上に配置
                    chunkSize * chunkCountZ / 2f
                );
                player.transform.position = spawnPos;
                Debug.Log($"[MC_World] プレイヤーを固定位置にスポーン: {spawnPos}");
            }
        }
    }

    public void Dig(Vector3 worldPos, float radius, float value = 0f)
    {
        Vector3 min = worldPos - Vector3.one * radius;
        Vector3 max = worldPos + Vector3.one * radius;

        Vector3Int minChunk = WorldToChunkCoord(min);
        Vector3Int maxChunk = WorldToChunkCoord(max);

        for (int x = minChunk.x; x <= maxChunk.x; x++)
            for (int y = minChunk.y; y <= maxChunk.y; y++)
                for (int z = minChunk.z; z <= maxChunk.z; z++)
                {
                    Vector3Int chunkCoord = new Vector3Int(x, y, z);
                    if (chunkMap.TryGetValue(chunkCoord, out var chunk))
                    {
                        chunk.ModifyDensity(worldPos, radius, value);
                        chunk.GenerateMesh();
                    }
                }
    }

    Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / chunkSize),
            Mathf.FloorToInt(worldPos.y / chunkSize),
            Mathf.FloorToInt(worldPos.z / chunkSize)
        );
    }
}