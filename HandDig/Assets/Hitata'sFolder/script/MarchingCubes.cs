using UnityEngine;
using System.Collections.Generic;

public class MarchingCubes
{
    public static Mesh GenerateMesh(float[,,] density, float isoLevel = 0f, float uvScale = 0.1f)
    {
        int sizeX = density.GetLength(0) - 1;
        int sizeY = density.GetLength(1) - 1;
        int sizeZ = density.GetLength(2) - 1;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // ���ɃV���v���ȗ�i1�̎O�p�`�ŃX�p�[�X�ɕ\���j
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < sizeZ; z++)
                {
                    if (density[x, y, z] < isoLevel) continue;

                    Vector3 pos = new Vector3(x, y, z);
                    vertices.Add(pos);
                    vertices.Add(pos + Vector3.up);
                    vertices.Add(pos + Vector3.right);

                    // UV座標を追加
                    uvs.Add(new Vector2(pos.x * uvScale, pos.z * uvScale));
                    uvs.Add(new Vector2((pos.x + Vector3.up.x) * uvScale, (pos.z + Vector3.up.z) * uvScale));
                    uvs.Add(new Vector2((pos.x + Vector3.right.x) * uvScale, (pos.z + Vector3.right.z) * uvScale));

                    int index = vertices.Count - 3;
                    triangles.Add(index);
                    triangles.Add(index + 1);
                    triangles.Add(index + 2);
                }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }
}
