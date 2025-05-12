using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public Camera mainCamera; // カメラ（VRの場合はコントローラのTransformなどに変更可）
    public float brushRadius = 2f;
    public float digStrength = 1f;

    void Update()
    {
        if (Input.GetMouseButton(0)) // 左クリックで掘削（VRでは代替入力に）
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                MC_Chunk chunk = hit.collider.GetComponent<MC_Chunk>();
                if (chunk != null)
                {
                    ModifyDensity(chunk, hit.point);
                    chunk.GenerateMesh();
                }
            }
        }
    }

    void ModifyDensity(MC_Chunk chunk, Vector3 worldPos)
    {
        Vector3 localPos = worldPos - chunk.transform.position;
        float voxelScale = chunk.chunkData.voxelScale;
        int chunkSize = chunk.chunkData.chunkSize;

        int brushVoxels = Mathf.CeilToInt(brushRadius / voxelScale);

        for (int x = -brushVoxels; x <= brushVoxels; x++)
        {
            for (int y = -brushVoxels; y <= brushVoxels; y++)
            {
                for (int z = -brushVoxels; z <= brushVoxels; z++)
                {
                    Vector3 offset = new Vector3(x, y, z) * voxelScale;
                    Vector3 p = localPos + offset;

                    int vx = Mathf.RoundToInt(p.x / voxelScale);
                    int vy = Mathf.RoundToInt(p.y / voxelScale);
                    int vz = Mathf.RoundToInt(p.z / voxelScale);

                    if (vx >= 0 && vy >= 0 && vz >= 0 &&
                        vx < chunkSize + 1 && vy < chunkSize + 1 && vz < chunkSize + 1)
                    {
                        float distance = Vector3.Distance(p, localPos);
                        if (distance < brushRadius)
                        {
                            float strength = Mathf.Clamp01(1f - distance / brushRadius) * digStrength;
                            chunk.chunkData.densityMap[vx, vy, vz] -= strength;
                        }
                    }
                }
            }
        }
    }
}
