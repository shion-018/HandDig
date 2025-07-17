using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeHitZone : MonoBehaviour
{
    public PickaxeDigToolMaster masterTool;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
        {
            Debug.Log("[PickaxeHitZone] Terrainと衝突しました");
            
            if (masterTool != null)
            {
                bool isSwingReady = masterTool.IsSwingReady();
                Debug.Log($"[PickaxeHitZone] isSwingReady: {isSwingReady}");
                
                if (isSwingReady)
                {
                    masterTool.OnAnyHit();
                    Debug.Log("[PickaxeHitZone] 掘り実行！");
                }
                else
                {
                    Debug.Log("[PickaxeHitZone] 振りかぶり準備ができていません");
                }
            }
            else
            {
                Debug.LogError("[PickaxeHitZone] masterToolが設定されていません");
            }
        }
    }
}