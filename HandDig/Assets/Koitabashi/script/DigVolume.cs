//今のところスフィアとボックスコライダーにしか対応してない
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DigVolume : MonoBehaviour
{
    public float digValue = 0f;
    
    [Tooltip("1フレームあたりの処理するボクセル数")]
    public int voxelsPerFrame = 50; // 適度な値に

    // ApplyDigAsyncをpublicに
    public async UniTask ApplyDigAsync(MC_World world)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        int chunkSize = world.chunkSize;
        
        Debug.Log($"[DigVolume] 掘削開始: {bounds.size} (コライダータイプ: {col.GetType().Name}, チャンクサイズ: {chunkSize})");

        // コライダーのboundsから影響範囲のチャンクを特定
        Vector3Int minChunk = world.WorldToChunkCoord(bounds.min);
        Vector3Int maxChunk = world.WorldToChunkCoord(bounds.max);

        int processedVoxels = 0;
        int totalVoxels = 0;
        HashSet<MC_Chunk> modifiedChunks = new HashSet<MC_Chunk>();

        // 各チャンクを処理
        for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
        {
            for (int chunkY = minChunk.y; chunkY <= maxChunk.y; chunkY++)
            {
                for (int chunkZ = minChunk.z; chunkZ <= maxChunk.z; chunkZ++)
                {
                    Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);
                    MC_Chunk chunk = world.GetChunk(chunkCoord);
                    if (chunk == null || chunk.chunkData == null) continue;

                    // チャンクの実際のtransform.positionを使用（計算ではなく実際の位置）
                    Vector3 chunkWorldPos = chunk.transform.position;
                    
                    // チャンクのワールド座標範囲を計算（浮動小数点誤差を考慮して少し余裕を持たせる）
                    Vector3 chunkMin = chunkWorldPos;
                    Vector3 chunkMax = chunkWorldPos + new Vector3(chunkSize, chunkSize, chunkSize);
                    
                    // チャンクとコライダーのboundsが重なっているかチェック
                    if (bounds.max.x < chunkMin.x - 0.5f || bounds.min.x > chunkMax.x + 0.5f ||
                        bounds.max.y < chunkMin.y - 0.5f || bounds.min.y > chunkMax.y + 0.5f ||
                        bounds.max.z < chunkMin.z - 0.5f || bounds.min.z > chunkMax.z + 0.5f)
                    {
                        continue; // チャンクとコライダーが重なっていない
                    }
                    
                    // チャンク内のボクセルを処理（整数座標で）
                    for (int x = 0; x <= chunkSize; x++)
                    {
                        for (int y = 0; y <= chunkSize; y++)
                        {
                            for (int z = 0; z <= chunkSize; z++)
                            {
                                Vector3 voxelWorldPos = chunkWorldPos + new Vector3(x, y, z);
                                
                                // コライダータイプに応じた詳細判定（bounds.Contains()は使わず直接判定）
                                if (IsPointInsideCollider(col, voxelWorldPos))
                                {
                                    chunk.chunkData.densityMap[x, y, z] = digValue;
                                    totalVoxels++;
                                    modifiedChunks.Add(chunk);
                                    
                                    processedVoxels++;
                                    if (processedVoxels >= voxelsPerFrame)
                                    {
                                        processedVoxels = 0;
                                        await UniTask.Yield();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 変更されたチャンクのメッシュを再生成
        foreach (var chunk in modifiedChunks)
        {
            chunk.GenerateMesh();
        }

        Debug.Log($"[DigVolume] 掘削完了: {totalVoxels}個のボクセルを処理 ({modifiedChunks.Count}個のチャンクを更新)");
        // SpawnPointMarkerが付いている場合は消さない
        if (GetComponent<SpawnPointMarker>() == null)
        {
            Destroy(gameObject);
        }
    }

    // ApplyDigAsyncの同期版
    public void ApplyDigSync(MC_World world)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        int chunkSize = world.chunkSize;
        
        Debug.Log($"[DigVolume] 掘削開始: {bounds.size} (同期, コライダータイプ: {col.GetType().Name}, チャンクサイズ: {chunkSize})");

        // コライダーのboundsから影響範囲のチャンクを特定
        Vector3Int minChunk = world.WorldToChunkCoord(bounds.min);
        Vector3Int maxChunk = world.WorldToChunkCoord(bounds.max);

        int totalVoxels = 0;
        HashSet<MC_Chunk> modifiedChunks = new HashSet<MC_Chunk>();

        // 各チャンクを処理
        for (int chunkX = minChunk.x; chunkX <= maxChunk.x; chunkX++)
        {
            for (int chunkY = minChunk.y; chunkY <= maxChunk.y; chunkY++)
            {
                for (int chunkZ = minChunk.z; chunkZ <= maxChunk.z; chunkZ++)
                {
                    Vector3Int chunkCoord = new Vector3Int(chunkX, chunkY, chunkZ);
                    MC_Chunk chunk = world.GetChunk(chunkCoord);
                    if (chunk == null || chunk.chunkData == null) continue;

                    // チャンクの実際のtransform.positionを使用（計算ではなく実際の位置）
                    Vector3 chunkWorldPos = chunk.transform.position;
                    
                    // チャンクのワールド座標範囲を計算（浮動小数点誤差を考慮して少し余裕を持たせる）
                    Vector3 chunkMin = chunkWorldPos;
                    Vector3 chunkMax = chunkWorldPos + new Vector3(chunkSize, chunkSize, chunkSize);
                    
                    // チャンクとコライダーのboundsが重なっているかチェック
                    if (bounds.max.x < chunkMin.x - 0.5f || bounds.min.x > chunkMax.x + 0.5f ||
                        bounds.max.y < chunkMin.y - 0.5f || bounds.min.y > chunkMax.y + 0.5f ||
                        bounds.max.z < chunkMin.z - 0.5f || bounds.min.z > chunkMax.z + 0.5f)
                    {
                        continue; // チャンクとコライダーが重なっていない
                    }
                    
                    // チャンク内のボクセルを処理（整数座標で）
                    for (int x = 0; x <= chunkSize; x++)
                    {
                        for (int y = 0; y <= chunkSize; y++)
                        {
                            for (int z = 0; z <= chunkSize; z++)
                            {
                                Vector3 voxelWorldPos = chunkWorldPos + new Vector3(x, y, z);
                                
                                // コライダータイプに応じた詳細判定（bounds.Contains()は使わず直接判定）
                                if (IsPointInsideCollider(col, voxelWorldPos))
                                {
                                    chunk.chunkData.densityMap[x, y, z] = digValue;
                                    totalVoxels++;
                                    modifiedChunks.Add(chunk);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 変更されたチャンクのメッシュを再生成
        foreach (var chunk in modifiedChunks)
        {
            chunk.GenerateMesh();
        }

        Debug.Log($"[DigVolume] 掘削完了: {totalVoxels}個のボクセルを処理 ({modifiedChunks.Count}個のチャンクを更新, 同期)");
        // SpawnPointMarkerが付いている場合は消さない
        if (GetComponent<SpawnPointMarker>() == null)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// コライダータイプに応じた点の内部判定
    /// </summary>
    private bool IsPointInsideCollider(Collider col, Vector3 worldPos)
    {
        if (col is BoxCollider boxCollider)
        {
            return IsPointInsideBoxCollider(boxCollider, worldPos);
        }
        else if (col is SphereCollider sphereCollider)
        {
            return IsPointInsideSphereCollider(sphereCollider, worldPos);
        }
        else if (col is CapsuleCollider capsuleCollider)
        {
            return IsPointInsideCapsuleCollider(capsuleCollider, worldPos);
        }
        else
        {
            // その他のコライダーは従来の方法を使用
            return col.bounds.Contains(worldPos) && col.ClosestPoint(worldPos) == worldPos;
        }
    }
    
    /// <summary>
    /// BoxColliderの内部判定（ローカル座標で判定）
    /// </summary>
    private bool IsPointInsideBoxCollider(BoxCollider boxCollider, Vector3 worldPos)
    {
        // ワールド座標をローカル座標に変換
        Vector3 localPos = boxCollider.transform.InverseTransformPoint(worldPos);
        
        // ローカル座標で -size/2 ～ size/2 の範囲内かチェック
        Vector3 halfSize = boxCollider.size * 0.5f;
        
        return Mathf.Abs(localPos.x) <= halfSize.x &&
               Mathf.Abs(localPos.y) <= halfSize.y &&
               Mathf.Abs(localPos.z) <= halfSize.z;
    }
    
    /// <summary>
    /// SphereColliderの内部判定
    /// </summary>
    private bool IsPointInsideSphereCollider(SphereCollider sphereCollider, Vector3 worldPos)
    {
        // ワールド座標をローカル座標に変換
        Vector3 localPos = sphereCollider.transform.InverseTransformPoint(worldPos);
        
        // スケールを考慮した半径を計算
        float scaledRadius = sphereCollider.radius * Mathf.Max(
            Mathf.Abs(sphereCollider.transform.lossyScale.x),
            Mathf.Abs(sphereCollider.transform.lossyScale.y),
            Mathf.Abs(sphereCollider.transform.lossyScale.z)
        );
        
        // 中心からの距離が半径以下かチェック
        return localPos.magnitude <= scaledRadius;
    }
    
    /// <summary>
    /// CapsuleColliderの内部判定
    /// </summary>
    private bool IsPointInsideCapsuleCollider(CapsuleCollider capsuleCollider, Vector3 worldPos)
    {
        // ワールド座標をローカル座標に変換
        Vector3 localPos = capsuleCollider.transform.InverseTransformPoint(worldPos);
        
        // カプセルの軸に応じて判定
        Vector3 axis = Vector3.zero;
        switch (capsuleCollider.direction)
        {
            case 0: axis = Vector3.right; break;   // X軸
            case 1: axis = Vector3.up; break;      // Y軸
            case 2: axis = Vector3.forward; break; // Z軸
        }
        
        // 軸方向の距離と半径方向の距離を計算
        float axisDistance = Vector3.Dot(localPos, axis);
        float radiusDistance = Vector3.Distance(localPos, axis * axisDistance);
        
        // カプセルの高さと半径を考慮
        float halfHeight = capsuleCollider.height * 0.5f;
        float scaledRadius = capsuleCollider.radius * Mathf.Max(
            Mathf.Abs(capsuleCollider.transform.lossyScale.x),
            Mathf.Abs(capsuleCollider.transform.lossyScale.y),
            Mathf.Abs(capsuleCollider.transform.lossyScale.z)
        );
        
        return Mathf.Abs(axisDistance) <= halfHeight && radiusDistance <= scaledRadius;
    }
}