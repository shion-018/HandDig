using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DigVolume : MonoBehaviour
{
    public float digValue = 0f;
    
    [Tooltip("1フレームあたりの処理するボクセル数")]
    public int voxelsPerFrame = 50; // 適度な値に

    // ApplyDigAsyncをpublicに
    public async UniTask ApplyDigAsync(MC_World world)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        
        Debug.Log($"[DigVolume] 掘削開始: {bounds.size}");

        int processedVoxels = 0;
        int totalVoxels = 0;

        // 元の密度で掘削
        for (float x = bounds.min.x; x <= bounds.max.x; x += 1f)
        {
            for (float y = bounds.min.y; y <= bounds.max.y; y += 1f)
            {
                for (float z = bounds.min.z; z <= bounds.max.z; z += 1f)
                {
                    Vector3 p = new Vector3(x, y, z);
                    if (col.bounds.Contains(p) && col.ClosestPoint(p) == p)
                    {
                        world.Dig(p, 0.5f, digValue);
                        processedVoxels++;
                        totalVoxels++;
                        if (processedVoxels >= voxelsPerFrame)
                        {
                            processedVoxels = 0;
                            await UniTask.Yield();
                        }
                    }
                }
            }
        }

        Debug.Log($"[DigVolume] 掘削完了: {totalVoxels}個のボクセルを処理");
        // SpawnPointMarkerが付いている場合は消さない
        if (GetComponent<SpawnPointMarker>() == null)
        {
            Destroy(gameObject);
        }
    }
}