using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_ChunkManager : MonoBehaviour
{
    public Dictionary<Vector3Int, MC_Chunk> chunks = new();
    public GameObject chunkPrefab;
    public int chunkCountX = 4;
    public int chunkCountY = 2;
    public int chunkCountZ = 4;
    public int chunkSize = 32;

    void Start()
    {
        for (int x = 0; x < chunkCountX; x++)
            for (int y = 0; y < chunkCountY; y++)
                for (int z = 0; z < chunkCountZ; z++)
                {
                    Vector3Int pos = new Vector3Int(x * chunkSize, -y * chunkSize, z * chunkSize);
                    GameObject chunkObj = Instantiate(chunkPrefab, pos, Quaternion.identity, transform);
                    MC_Chunk chunk = chunkObj.GetComponent<MC_Chunk>();
                    chunk.chunkSize = chunkSize;
                    chunk.Initialize(pos);
                    chunks[pos] = chunk;
                }
    }

    public MC_Chunk GetChunkAt(Vector3Int pos)
    {
        chunks.TryGetValue(pos, out var chunk);
        return chunk;
    }

    public Vector3Int GetChunkCoordFromWorldPos(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / chunkSize) * chunkSize,
            Mathf.FloorToInt(worldPos.y / chunkSize) * chunkSize,
            Mathf.FloorToInt(worldPos.z / chunkSize) * chunkSize
        );
    }
}