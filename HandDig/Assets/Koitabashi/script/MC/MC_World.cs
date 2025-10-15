using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;


public class MC_World : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 32;
    public int chunkCountX = 1;
    public int chunkCountY = 5;
    public int chunkCountZ = 1;
    public LayerMask buriedObjectLayer; // 埋蔵物の判定に使う
    public LayerMask terrainLayer; // 地形の露出判定に使う
    private CancellationTokenSource cancellationTokenSource;

    [Header("初期化方式切り替え")]
    [Tooltip("true: 分散処理 (非同期)、false: 一気に同期処理")]
    public bool useAsyncInitialization = true;

    [Tooltip("お宝を生成するTreasureSpawnerのリスト")]
    public List<TreasureSpawner> treasureSpawners = new List<TreasureSpawner>();

    [Header("つるはし専用地面設定")]
    [Tooltip("つるはし専用地面のプレハブ")]
    public GameObject pickaxeChunkPrefab;
    
    [Tooltip("つるはし専用地面の中心地点（Transformのリスト）")]
    public List<Transform> pickaxeTerrainCenters = new List<Transform>();
    
    [Tooltip("各地点のチャンク半径")]
    public List<int> pickaxeTerrainRadii = new List<int>();

    public List<DigVolume> digVolumesToApply;

    Dictionary<Vector3Int, MC_Chunk> chunkMap = new Dictionary<Vector3Int, MC_Chunk>();

    void Start()
    {
        Debug.Log("[MC_World] ワールド初期化開始");
        // 非同期初期化のみ実行
        //InitializeWorldAsync().Forget();
        cancellationTokenSource = new CancellationTokenSource();
        InitializeWorldAsync(cancellationTokenSource.Token).Forget();
    }

    async UniTask InitializeWorldAsync(CancellationToken token)
    {
        // 各TreasureSpawnerにchunkSizeを適用
        foreach (var spawner in treasureSpawners)
        {
            if (spawner == null) continue;
            spawner.chunkSize = chunkSize;
        }

        // チャンク生成を非同期で実行
        await GenerateChunksAsync(token);

        // DigVolumeの処理を非同期で実行
        await ApplyDigVolumesAsync();

        // スポーンシステムの処理
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager == null)
        {
            // 従来の固定スポーン処理（後方互換性のため残す）
            Vector3 startDigLocal = new Vector3(
                chunkSize * chunkCountX / 2f,
                -chunkSize * 2,
                chunkSize * chunkCountZ / 2f
            );
            Vector3 startDigPos = transform.position + startDigLocal;
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

    async UniTask GenerateChunksAsync(CancellationToken token)
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
                    Vector3 localPos = new Vector3(
                        x * chunkSize,
                        shiftedY * chunkSize,
                        z * chunkSize
                    );
                    Vector3 worldPos = transform.position + localPos;

                    // つるはし専用エリアかどうかを判定
                    bool isPickaxeOnly = IsPickaxeOnlyChunk(pos);
                    GameObject prefabToUse = isPickaxeOnly ? pickaxeChunkPrefab : chunkPrefab;
                    
                    GameObject obj = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);
                    MC_Chunk chunk = obj.GetComponent<MC_Chunk>();
                    chunk.Initialize(worldPos);
                    chunkMap[pos] = chunk;
                    
                    // デバッグログ
                    if (isPickaxeOnly)
                    {
                        Debug.Log($"[MC_World] つるはし専用チャンク生成: {pos}, プレハブ: {prefabToUse.name}, タグ: {obj.tag}");
                    }

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
        await UniTask.Yield(PlayerLoopTiming.Update, token);
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

    /// <summary>
    /// 指定チャンクがつるはし専用エリアかどうかを判定
    /// </summary>
    private bool IsPickaxeOnlyChunk(Vector3Int chunkCoord)
    {
        if (pickaxeChunkPrefab == null || pickaxeTerrainCenters.Count == 0)
            return false;
        
        for (int i = 0; i < pickaxeTerrainCenters.Count; i++)
        {
            if (pickaxeTerrainCenters[i] == null) continue;
            
            Vector3 centerPos = pickaxeTerrainCenters[i].position;
            Vector3Int centerChunk = WorldToChunkCoord(centerPos);
            int radius = (i < pickaxeTerrainRadii.Count) ? pickaxeTerrainRadii[i] : 2;
            
            int distanceX = Mathf.Abs(chunkCoord.x - centerChunk.x);
            int distanceY = Mathf.Abs(chunkCoord.y - centerChunk.y);
            int distanceZ = Mathf.Abs(chunkCoord.z - centerChunk.z);
            
            if (distanceX <= radius && distanceY <= radius && distanceZ <= radius)
            {
                return true;
            }
        }
        
        return false;
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
                Vector3 spawnLocal = new Vector3(
                    chunkSize * chunkCountX / 2f,
                    10f, // 少し上に配置
                    chunkSize * chunkCountZ / 2f
                );
                Vector3 spawnPos = transform.position + spawnLocal;
                player.transform.position = spawnPos;
                Debug.Log($"[MC_World] プレイヤーを固定位置にスポーン: {spawnPos}");
            }
        }
    }
    Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        // ワールド原点ではなく、このMC_Worldの原点（transform.position）を基準にローカル換算
        Vector3 local = worldPos - transform.position;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / chunkSize),
            Mathf.FloorToInt(local.y / chunkSize),
            Mathf.FloorToInt(local.z / chunkSize)
        );

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
        
        // ★掘削後の埋蔵物チェック
        CheckForExposedBuriedObjects(worldPos, radius);
    }

    /// <summary>
    /// 掘削を試行し、実際に掘削が発生したかどうかを返す
    /// </summary>
    /// <param name="worldPos">掘削位置</param>
    /// <param name="radius">掘削半径</param>
    /// <param name="value">掘削値（デフォルト0）</param>
    /// <returns>実際に掘削が発生した場合true</returns>
    public bool TryDig(Vector3 worldPos, float radius, float value = 0f)
    {
        Vector3 min = worldPos - Vector3.one * radius;
        Vector3 max = worldPos + Vector3.one * radius;

        Vector3Int minChunk = WorldToChunkCoord(min);
        Vector3Int maxChunk = WorldToChunkCoord(max);

        bool digOccurred = false;

        for (int x = minChunk.x; x <= maxChunk.x; x++)
            for (int y = minChunk.y; y <= maxChunk.y; y++)
                for (int z = minChunk.z; z <= maxChunk.z; z++)
                {
                    Vector3Int chunkCoord = new Vector3Int(x, y, z);
                    if (chunkMap.TryGetValue(chunkCoord, out var chunk))
                    {
                        // 掘削前の状態をチェック
                        bool hadVoxels = chunk.HasVoxelsInRange(worldPos, radius);
                        
                        if (hadVoxels)
                        {
                            chunk.ModifyDensity(worldPos, radius, value);
                            chunk.GenerateMesh();
                            digOccurred = true;
                        }
                    }
                }
        
        // 掘削が発生した場合のみ埋蔵物チェック
        if (digOccurred)
        {
            CheckForExposedBuriedObjects(worldPos, radius);
        }

        return digOccurred;
    }

    
    void CheckForExposedBuriedObjects(Vector3 position, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius, buriedObjectLayer);
        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null && rb.isKinematic)
            {
                if (IsExposed(hit.transform.position))
                {
                    rb.isKinematic = false;
                }
            }
        }
    }

    bool IsExposed(Vector3 pos)
    {
        float checkDistance = 0.4f;
        int exposedSides = 0;

        Vector3[] directions = new Vector3[]
        {
        Vector3.up, Vector3.down,
        Vector3.left, Vector3.right,
        Vector3.forward, Vector3.back
        };

        foreach (var dir in directions)
        {
            Vector3 checkPos = pos + dir * checkDistance;
            if (!Physics.CheckSphere(checkPos, 0.2f, terrainLayer))
            {
                exposedSides++;
            }
        }

        return exposedSides >= 3; // 3面以上空いていたら「露出」と判断
    }
    void OnDestroy()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}