using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;


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

    [Header("Prebaked 読み込み設定")]
    [Tooltip("起動時に事前生成データを使う（密度生成とDigVolumeをスキップ）")]
    public bool usePrebakedData = false;

    [Tooltip("チャンク座標→事前生成データの対応（サイズはchunkCountX*chunkCountY*chunkCountZ）")]
    public List<MC_ChunkDataAsset> prebakedAssets = new List<MC_ChunkDataAsset>();

    Dictionary<Vector3Int, MC_Chunk> chunkMap = new Dictionary<Vector3Int, MC_Chunk>();

    /// <summary>
    /// 初期化が完了したかどうか
    /// </summary>
    public bool IsInitialized { get; private set; } = false;

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
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();

        // 各TreasureSpawnerにchunkSizeとコンテキストを適用
        foreach (var spawner in treasureSpawners)
        {
            if (spawner == null) continue;
            spawner.chunkSize = chunkSize;
            spawner.InitializeContext(this, spawnManager);
        }

        // チャンク生成を非同期で実行
        await GenerateChunksAsync(token);

        // 事前生成データを使わない場合のみ掘削ボリュームを適用
        if (!usePrebakedData)
        {
            await ApplyDigVolumesAsync();
        }

        // 掘削後の密度を参照してからお宝を生成
        SpawnTreasuresAcrossWorld();

        // スポーンシステムの処理
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
            Debug.Log("[MC_World] SpawnManagerが設定されています。固定スポーンポイントを使用します。");
        }
        
        // プレイヤーを実際のスポーン位置に移動
        SpawnPlayerAtFinalPosition();
        
        // 初期化完了フラグを設定
        IsInitialized = true;
        
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
                    
                    // MC_WorldのchunkSizeを各チャンクに適用
                    chunk.chunkSize = chunkSize;

                    // 事前生成データが有効なら、対応するアセットを割り当て
                    if (usePrebakedData)
                    {
                        int linearIndex = ToLinearIndex(x, y, z, chunkCountX, chunkCountY, chunkCountZ);
                        if (linearIndex >= 0 && linearIndex < prebakedAssets.Count)
                        {
                            chunk.prebakedData = prebakedAssets[linearIndex];
                        }
                    }

                    chunk.Initialize(worldPos);
                    chunkMap[pos] = chunk;
                    
                    // デバッグログ
                    if (isPickaxeOnly)
                    {
                        Debug.Log($"[MC_World] つるはし専用チャンク生成: {pos}, プレハブ: {prefabToUse.name}, タグ: {obj.tag}");
                    }

                    chunkMap[pos] = chunk;
                    
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

    void SpawnTreasuresAcrossWorld()
    {
        if (treasureSpawners == null || treasureSpawners.Count == 0)
            return;

        foreach (var kvp in chunkMap)
        {
            Vector3Int coord = kvp.Key;
            MC_Chunk chunk = kvp.Value;
            if (chunk == null) continue;

            foreach (var spawner in treasureSpawners)
            {
                if (spawner == null) continue;
                spawner.TrySpawnTreasureAtChunk(coord, chunk);
            }
        }

        Debug.Log("[MC_World] お宝のスポーン処理が完了しました");
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
    public Vector3Int WorldToChunkCoord(Vector3 worldPos)
    {
        // ワールド原点ではなく、このMC_Worldの原点（transform.position）を基準にローカル換算
        Vector3 local = worldPos - transform.position;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / chunkSize),
            Mathf.FloorToInt(local.y / chunkSize),
            Mathf.FloorToInt(local.z / chunkSize)
        );

    }

    /// <summary>
    /// チャンク座標からチャンクを取得
    /// </summary>
    public MC_Chunk GetChunk(Vector3Int chunkCoord)
    {
        chunkMap.TryGetValue(chunkCoord, out var chunk);
        return chunk;
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

    // 3次元インデックスを一次元に変換（Yは0..chunkCountY-1だが、生成時はshiftedY=-yで並べているため、引数はyそのまま）
    private int ToLinearIndex(int x, int y, int z, int sizeX, int sizeY, int sizeZ)
    {
        return x + sizeX * (y + sizeY * z);
    }

#if UNITY_EDITOR
    [ContextMenu("MC/Save All Chunks To Assets (PlayMode)")]
    public void SaveAllChunksToAssetsInEditor()
    {
        string folder = "Assets/MCPrebaked";
        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "MCPrebaked");
        }

        int total = chunkCountX * chunkCountY * chunkCountZ;
        if (prebakedAssets == null) prebakedAssets = new List<MC_ChunkDataAsset>(total);
        if (prebakedAssets.Count != total)
        {
            prebakedAssets.Clear();
            for (int i = 0; i < total; i++) prebakedAssets.Add(null);
        }

        int processed = 0;
        for (int x = 0; x < chunkCountX; x++)
        for (int y = 0; y < chunkCountY; y++)
        for (int z = 0; z < chunkCountZ; z++)
        {
            int shiftedY = -y;
            Vector3Int key = new Vector3Int(x, shiftedY, z);
            if (!chunkMap.TryGetValue(key, out var chunk) || chunk == null || chunk.chunkData == null)
            {
                processed++;
                continue;
            }

            // プログレスバーを更新
            float progress = (float)processed / total;
            string info = $"チャンクを保存中... ({x}, {y}, {z}) - {processed}/{total}";
            UnityEditor.EditorUtility.DisplayProgressBar("チャンクデータ保存", info, progress);

            string assetPath = System.IO.Path.Combine(folder, $"chunk_x{x}_y{y}_z{z}.asset");
            var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<MC_ChunkDataAsset>(assetPath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<MC_ChunkDataAsset>();
                UnityEditor.AssetDatabase.CreateAsset(existing, assetPath);
            }

            existing.FromRuntimeData(chunk.chunkData);
            UnityEditor.EditorUtility.SetDirty(existing);

            int idx = ToLinearIndex(x, y, z, chunkCountX, chunkCountY, chunkCountZ);
            prebakedAssets[idx] = existing;

            chunk.prebakedData = existing;
            UnityEditor.EditorUtility.SetDirty(chunk);
            
            processed++;
        }

        // 最後のプログレスバー更新
        UnityEditor.EditorUtility.DisplayProgressBar("チャンクデータ保存", "アセットを保存中...", 0.95f);

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        // プログレスバーをクリア
        UnityEditor.EditorUtility.ClearProgressBar();

        usePrebakedData = true;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[MC_World] 全チャンクをアセットに保存し、prebakedAssetsへ登録しました。 (処理チャンク数: {processed}/{total})");
    }

    [ContextMenu("MC/Assign PrebakedAssets From Folder")] 
    public void AssignPrebakedAssetsFromFolder()
    {
        string folderAbs = UnityEditor.EditorUtility.OpenFolderPanel("Select Prebaked Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(folderAbs)) return;

        // 絶対パスをプロジェクト相対(Assets/...)に変換
        string assetsAbs = Application.dataPath.Replace("\\", "/");
        folderAbs = folderAbs.Replace("\\", "/");
        if (!folderAbs.StartsWith(assetsAbs))
        {
            UnityEditor.EditorUtility.DisplayDialog("Assign Prebaked", "Assets配下のフォルダを選択してください", "OK");
            return;
        }
        string folderRel = "Assets" + folderAbs.Substring(assetsAbs.Length);

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MC_ChunkDataAsset", new string[] { folderRel });
        if (guids == null || guids.Length == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog("Assign Prebaked", "フォルダ内に MC_ChunkDataAsset が見つかりません", "OK");
            return;
        }

        int total = chunkCountX * chunkCountY * chunkCountZ;
        if (prebakedAssets == null) prebakedAssets = new List<MC_ChunkDataAsset>(total);
        if (prebakedAssets.Count != total)
        {
            prebakedAssets.Clear();
            for (int i = 0; i < total; i++) prebakedAssets.Add(null);
        }

        Regex namePattern = new Regex(@"chunk_x(\d+)_y(\d+)_z(\d+)", RegexOptions.IgnoreCase);
        int assigned = 0;
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<MC_ChunkDataAsset>(path);
            if (asset == null) continue;

            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            var m = namePattern.Match(fileName);
            if (!m.Success)
            {
                // 名前が規約外ならスキップ
                continue;
            }

            int x = int.Parse(m.Groups[1].Value);
            int y = int.Parse(m.Groups[2].Value);
            int z = int.Parse(m.Groups[3].Value);
            if (x < 0 || x >= chunkCountX || y < 0 || y >= chunkCountY || z < 0 || z >= chunkCountZ)
                continue;

            int idx = ToLinearIndex(x, y, z, chunkCountX, chunkCountY, chunkCountZ);
            prebakedAssets[idx] = asset;
            assigned++;

            // 既に生成済みのチャンクがあれば割当
            int shiftedY = -y;
            Vector3Int key = new Vector3Int(x, shiftedY, z);
            if (chunkMap.TryGetValue(key, out var chunk) && chunk != null)
            {
                chunk.prebakedData = asset;
                UnityEditor.EditorUtility.SetDirty(chunk);
            }
        }

        usePrebakedData = true;
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"[MC_World] フォルダ {folderRel} から {assigned} 件をprebakedAssetsへ割当しました");
    }
#endif

    [System.Serializable]
    private class ChunkFlat
    {
        public int width, height, depth, chunkSize;
        public float voxelScale;
        public float[] densityFlat;
    }

    [System.Serializable]
    private class PrebakedWorldData
    {
        public int countX, countY, countZ;
        public ChunkFlat[] chunks;
    }

    // 実行環境でも使えるJSON保存（persistentDataPath）
    public void SaveAllChunksToPersistent()
    {
        PrebakedWorldData world = new PrebakedWorldData
        {
            countX = chunkCountX,
            countY = chunkCountY,
            countZ = chunkCountZ,
            chunks = new ChunkFlat[chunkCountX * chunkCountY * chunkCountZ]
        };

        for (int x = 0; x < chunkCountX; x++)
        for (int y = 0; y < chunkCountY; y++)
        for (int z = 0; z < chunkCountZ; z++)
        {
            int shiftedY = -y;
            Vector3Int key = new Vector3Int(x, shiftedY, z);
            ChunkFlat cf = new ChunkFlat();
            if (chunkMap.TryGetValue(key, out var chunk) && chunk != null && chunk.chunkData != null)
            {
                var data = chunk.chunkData;
                cf.width = data.width;
                cf.height = data.height;
                cf.depth = data.depth;
                cf.chunkSize = data.chunkSize;
                cf.voxelScale = data.voxelScale;
                int sx = data.width + 1, sy = data.height + 1, sz = data.depth + 1;
                cf.densityFlat = new float[sx * sy * sz];
                int idxF = 0;
                for (int ix = 0; ix < sx; ix++)
                    for (int iy = 0; iy < sy; iy++)
                        for (int iz = 0; iz < sz; iz++)
                            cf.densityFlat[idxF++] = data.densityMap[ix, iy, iz];
            }
            int idx = ToLinearIndex(x, y, z, chunkCountX, chunkCountY, chunkCountZ);
            world.chunks[idx] = cf;
        }

        string json = JsonUtility.ToJson(world);
        string dir = Application.persistentDataPath;
        string path = System.IO.Path.Combine(dir, "mc_world_prebaked.json");
        File.WriteAllText(path, json);
        Debug.Log($"[MC_World] 保存: {path}");
    }

    public bool LoadAllChunksFromPersistent()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "mc_world_prebaked.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[MC_World] 読み込み対象なし: {path}");
            return false;
        }
        string json = File.ReadAllText(path);
        var world = JsonUtility.FromJson<PrebakedWorldData>(json);
        if (world == null || world.chunks == null) return false;

        // サイズが一致しない場合は失敗
        if (world.countX != chunkCountX || world.countY != chunkCountY || world.countZ != chunkCountZ)
        {
            Debug.LogWarning("[MC_World] ワールドサイズが一致しません");
            return false;
        }

        for (int x = 0; x < chunkCountX; x++)
        for (int y = 0; y < chunkCountY; y++)
        for (int z = 0; z < chunkCountZ; z++)
        {
            int idx = ToLinearIndex(x, y, z, chunkCountX, chunkCountY, chunkCountZ);
            var cf = world.chunks[idx];
            int shiftedY = -y;
            Vector3Int key = new Vector3Int(x, shiftedY, z);
            if (!chunkMap.TryGetValue(key, out var chunk) || chunk == null) continue;

            if (cf != null && cf.densityFlat != null)
            {
                var data = new MC_ChunkData(cf.width, cf.height, cf.depth, cf.chunkSize, cf.voxelScale);
                int sx = cf.width + 1, sy = cf.height + 1, sz = cf.depth + 1;
                int idxF = 0;
                for (int ix = 0; ix < sx; ix++)
                    for (int iy = 0; iy < sy; iy++)
                        for (int iz = 0; iz < sz; iz++)
                            data.densityMap[ix, iy, iz] = cf.densityFlat[idxF++];

                chunk.chunkData = data;
                chunk.GenerateMesh();
            }
        }

        Debug.Log("[MC_World] persistentから事前生成データを読み込み、メッシュを更新しました");
        return true;
    }
}