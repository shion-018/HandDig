using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MC_Chunk : MonoBehaviour
{
    public MC_ChunkData chunkData;
    public int chunkSize = 32;
    float baseHeight = 0f; // 全体のベース高さ（地表）
    float variation = 5f;   // 凹凸の程度（これを0にすると完全に平ら）

    public void Initialize(Vector3 position)
    {
        chunkData = new MC_ChunkData(chunkSize, chunkSize, chunkSize, chunkSize, 1f);
        transform.position = position;
        GenerateDensity();  // テスト的に地形データを入れる
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

        collider.sharedMesh = mesh; // 更新されたメッシュに合わせてコライダーを更新
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
}