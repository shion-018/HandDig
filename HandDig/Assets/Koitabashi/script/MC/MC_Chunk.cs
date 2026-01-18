using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MC_Chunk : MonoBehaviour
{
    public MC_ChunkData chunkData;
    public int chunkSize = 32;
    float baseHeight = 0f;
    float variation = 5f;

    public bool isExcluded=false;
    [Header("Prebaked Data")]
    public MC_ChunkDataAsset prebakedData;

    public void Initialize(Vector3 position)
    {
        // 事前生成データがあればそれを使用、なければ生成
        if (prebakedData != null && prebakedData.HasData)
        {
            chunkData = prebakedData.ToRuntimeData();
            chunkSize = prebakedData.chunkSize;
        }
        else
        {
            chunkData = new MC_ChunkData(chunkSize, chunkSize, chunkSize, chunkSize, 1f);
            GenerateDensity();
        }
        transform.position = position;
        GenerateMesh();
    }

    void GenerateDensity()
    {
        for (int x = 0; x <= chunkSize; x++)
            for (int y = 0; y <= chunkSize; y++)
                for (int z = 0; z <= chunkSize; z++)
                {
                    float worldY = transform.position.y + y;

                    float surfaceHeight = baseHeight + Mathf.PerlinNoise(
    (transform.position.x + x) * 0.05f,
    (transform.position.z + z) * 0.05f
) * variation;

                    // チャンクサイズに応じて適切な高さまで地形を生成
                    // 元の設定（チャンクサイズ32で基準高さ20）を基準に、チャンクサイズに比例して調整
                    // チャンクサイズが小さい場合、基準高さも低くして一番上のチャンク内で地形が生成されるようにする
                    float baseFillHeight = 20f * (chunkSize / 32f);
                    float baseFill = worldY < surfaceHeight || worldY < baseFillHeight ? 1f : 0f;
                    chunkData.densityMap[x, y, z] = baseFill;
                }
    }

    public void GenerateMesh()
    {
        Mesh mesh = MC_MeshGenerator.GenerateMesh(chunkData);
        GetComponent<MeshFilter>().mesh = mesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (!collider)
            collider = gameObject.AddComponent<MeshCollider>();

            collider.sharedMesh = mesh;
    }


    [Header("掘削範囲設定")]
    [Tooltip("下方向の掘削範囲の縮小倍率（0.0～1.0）")]
    [Range(0.0f, 1.0f)]
    public float lowerRadiusRatio = 0.3f;

    public void ModifyDensity(Vector3 worldPos, float radius, float value)
    {
        Vector3 localPos = worldPos - transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt(localPos.x - radius));
        int maxX = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(localPos.y - radius * lowerRadiusRatio));
        int maxY = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.y + radius));
        int minZ = Mathf.Max(0, Mathf.FloorToInt(localPos.z - radius));
        int maxZ = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.z + radius));

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 diff = new Vector3(x, y, z) - localPos;
                    
                    // 下方向の範囲を縮小
                    float effectiveRadius = diff.y < 0 ? radius * lowerRadiusRatio : radius;
                    
                    // 水平方向（x-z平面）の距離とY方向の距離を個別にチェック
                    float horizontalDist = Mathf.Sqrt(diff.x * diff.x + diff.z * diff.z);
                    float verticalDist = Mathf.Abs(diff.y);
                    
                    // 楕円形の判定：水平方向はradius、Y方向はeffectiveRadius
                    if (diff.y < 0)
                    {
                        // 下方向：楕円形判定
                        float normalizedH = horizontalDist / radius;
                        float normalizedV = verticalDist / effectiveRadius;
                        if (normalizedH * normalizedH + normalizedV * normalizedV <= 1.0f)
                        {
                            chunkData.densityMap[x, y, z] = value;
                        }
                    }
                    else
                    {
                        // 上方向：通常の球形判定
                        if (diff.magnitude <= radius)
                        {
                            chunkData.densityMap[x, y, z] = value;
                        }
                    }
                }
    }

    public bool HasVoxelsInRange(Vector3 worldPos, float radius)
    {
        Vector3 localPos = worldPos - transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt(localPos.x - radius));
        int maxX = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(localPos.y - radius * lowerRadiusRatio));
        int maxY = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.y + radius));
        int minZ = Mathf.Max(0, Mathf.FloorToInt(localPos.z - radius));
        int maxZ = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.z + radius));

        const float filledThreshold = 0.5f;
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 diff = new Vector3(x, y, z) - localPos;
                    
                    bool isInRange = false;
                    if (diff.y < 0)
                    {
                        // 下方向：楕円形判定
                        float effectiveRadius = radius * lowerRadiusRatio;
                        float horizontalDist = Mathf.Sqrt(diff.x * diff.x + diff.z * diff.z);
                        float verticalDist = Mathf.Abs(diff.y);
                        float normalizedH = horizontalDist / radius;
                        float normalizedV = verticalDist / effectiveRadius;
                        isInRange = (normalizedH * normalizedH + normalizedV * normalizedV <= 1.0f);
                    }
                    else
                    {
                        // 上方向：通常の球形判定
                        isInRange = (diff.magnitude <= radius);
                    }
                    
                    if (isInRange && chunkData != null && chunkData.densityMap[x, y, z] > filledThreshold)
                    {
                        return true;
                    }
                }

        return false;
    }
}