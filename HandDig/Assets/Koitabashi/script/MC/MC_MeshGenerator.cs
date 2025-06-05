using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_MeshGenerator
{
    public static Mesh GenerateMesh(MC_ChunkData chunkData, float surfaceLevel = 0.5f)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < chunkData.width; x++)
        {
            for (int y = 0; y < chunkData.height; y++)
            {
                for (int z = 0; z < chunkData.depth; z++)
                {
                    Vector3 position = new Vector3(x, y, z);
                    float[] cube = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 corner = position + MyMarchingCubes.CornerTable[i];
                        int cx = (int)corner.x;
                        int cy = (int)corner.y;
                        int cz = (int)corner.z;

                        if (cx < 0 || cy < 0 || cz < 0 || cx > chunkData.width || cy > chunkData.height || cz > chunkData.depth)
                            continue;

                        cube[i] = chunkData.densityMap[cx, cy, cz];
                    }

                    MyMarchingCubes.Polygonise(position, cube, surfaceLevel, vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}

