using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyMarchingCubes
{
    public static readonly Vector3[] CornerTable = new Vector3[8]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 0, 1),
        new Vector3(0, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1)
    };

    public static readonly int[,] EdgeIndexTable = new int[12, 2]
    {
        {0,1}, {1,2}, {2,3}, {3,0},
        {4,5}, {5,6}, {6,7}, {7,4},
        {0,4}, {1,5}, {2,6}, {3,7}
    };

    public static void Polygonise(Vector3 position, float[] cube, float surfaceLevel, List<Vector3> vertices, List<int> triangles)
    {
        int cubeIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cube[i] < surfaceLevel)
                cubeIndex |= 1 << i;
        }

        int edges = MyMarchingTable.EdgeTable[cubeIndex];
        if (edges == 0)
            return;

        Vector3[] vertList = new Vector3[12];

        for (int i = 0; i < 12; i++)
        {
            if ((edges & (1 << i)) != 0)
            {
                int a0 = EdgeIndexTable[i, 0];
                int b0 = EdgeIndexTable[i, 1];

                Vector3 p1 = position + CornerTable[a0];
                Vector3 p2 = position + CornerTable[b0];

                float val1 = cube[a0];
                float val2 = cube[b0];

                vertList[i] = InterpolateVerts(p1, p2, val1, val2, surfaceLevel);
            }
        }

        for (int i = 0; MyMarchingTable.TriangleTable[cubeIndex, i] != -1; i += 3)
        {
            int idx0 = AddVertex(vertList[MyMarchingTable.TriangleTable[cubeIndex, i]], vertices);
            int idx1 = AddVertex(vertList[MyMarchingTable.TriangleTable[cubeIndex, i + 1]], vertices);
            int idx2 = AddVertex(vertList[MyMarchingTable.TriangleTable[cubeIndex, i + 2]], vertices);

            triangles.Add(idx0);
            triangles.Add(idx2);
            triangles.Add(idx1);
        }
    }

    private static Vector3 InterpolateVerts(Vector3 p1, Vector3 p2, float val1, float val2, float surfaceLevel)
    {
        float t = (surfaceLevel - val1) / (val2 - val1);
        return Vector3.Lerp(p1, p2, t);
    }

    private static int AddVertex(Vector3 vertex, List<Vector3> vertices)
    {
        int index = vertices.Count;
        vertices.Add(vertex);
        return index;
    }
}