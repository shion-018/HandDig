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

                    float baseFill = worldY < surfaceHeight || worldY < 20 ? 1f : 0f;
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


    public void ModifyDensity(Vector3 worldPos, float radius, float value)
    {
        Vector3 localPos = worldPos - transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt(localPos.x - radius));
        int maxX = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(localPos.y - radius));
        int maxY = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.y + radius));
        int minZ = Mathf.Max(0, Mathf.FloorToInt(localPos.z - radius));
        int maxZ = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.z + radius));

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 diff = new Vector3(x, y, z) - localPos;
                    if (diff.magnitude <= radius)
                        chunkData.densityMap[x, y, z] = value;
                }
    }

    public bool HasVoxelsInRange(Vector3 worldPos, float radius)
    {
        Vector3 localPos = worldPos - transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt(localPos.x - radius));
        int maxX = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.x + radius));
        int minY = Mathf.Max(0, Mathf.FloorToInt(localPos.y - radius));
        int maxY = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.y + radius));
        int minZ = Mathf.Max(0, Mathf.FloorToInt(localPos.z - radius));
        int maxZ = Mathf.Min(chunkSize, Mathf.CeilToInt(localPos.z + radius));

        const float filledThreshold = 0.5f;
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 diff = new Vector3(x, y, z) - localPos;
                    if (diff.magnitude <= radius)
                    {
                        if (chunkData != null && chunkData.densityMap[x, y, z] > filledThreshold)
                            return true;
                    }
                }

        return false;
    }
}