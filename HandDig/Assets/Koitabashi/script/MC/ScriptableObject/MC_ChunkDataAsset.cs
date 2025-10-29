using UnityEngine;

[CreateAssetMenu(menuName = "MC/Chunk Data Asset", fileName = "MC_ChunkDataAsset")]
public class MC_ChunkDataAsset : ScriptableObject
{
    public int width = 32;
    public int height = 32;
    public int depth = 32;
    public int chunkSize = 32;
    public float voxelScale = 1f;

    // 直列化用フラット配列（(width+1)*(height+1)*(depth+1)）
    [SerializeField]
    private float[] densityFlat;

    public bool HasData => densityFlat != null && densityFlat.Length == (width + 1) * (height + 1) * (depth + 1);

    public void FromRuntimeData(MC_ChunkData data)
    {
        width = data.width;
        height = data.height;
        depth = data.depth;
        chunkSize = data.chunkSize;
        voxelScale = data.voxelScale;

        int sx = width + 1;
        int sy = height + 1;
        int sz = depth + 1;
        densityFlat = new float[sx * sy * sz];
        int idx = 0;
        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                {
                    densityFlat[idx++] = data.densityMap[x, y, z];
                }
    }

    public MC_ChunkData ToRuntimeData()
    {
        MC_ChunkData data = new MC_ChunkData(width, height, depth, chunkSize, voxelScale);
        if (!HasData)
        {
            return data;
        }

        int sx = width + 1;
        int sy = height + 1;
        int sz = depth + 1;
        int idx = 0;
        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                {
                    data.densityMap[x, y, z] = densityFlat[idx++];
                }
        return data;
    }
}



