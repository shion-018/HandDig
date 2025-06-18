using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_World : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int chunkSize = 32;
    public int chunkCountX = 1;
    public int chunkCountY = 5;
    public int chunkCountZ = 1;

    public TreasureSpawner treasureSpawner;
    public List<DigVolume> digVolumesToApply;

    Dictionary<Vector3Int, MC_Chunk> chunkMap = new Dictionary<Vector3Int, MC_Chunk>();

    void Start()
    {
        treasureSpawner.chunkSize = chunkSize;
        Vector3Int center = new Vector3Int(chunkCountX / 2, -2, chunkCountZ / 2);
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                Vector3Int excluded = new Vector3Int(center.x + dx, center.y, center.z + dz);
                treasureSpawner.AddExcludedChunk(excluded);
            }
        GenerateChunks();
        foreach (var vol in digVolumesToApply)
        {
            vol.ApplyDig(this);
        }
        // スタート地点（ワールド中心）を広めに掘って空間に
        Vector3 startDigPos = new Vector3(
            chunkSize * chunkCountX / 2f,
            -chunkSize * 2, // 地下2チャンク分下
            chunkSize * chunkCountZ / 2f
        );
        Dig(startDigPos, 10f);
    }

    void GenerateChunks()
    {
        for (int x = 0; x < chunkCountX; x++)
            for (int y = 0; y < chunkCountY; y++)
                for (int z = 0; z < chunkCountZ; z++)
                {
                    // Y方向だけ反転させて、Y=0 から下に掘る構成にする
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

                    treasureSpawner.TrySpawnTreasureAtChunk(pos, worldPos);

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
                        chunk.GenerateMesh(); // 掘った後に更新
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