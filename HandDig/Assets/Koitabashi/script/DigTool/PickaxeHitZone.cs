using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeHitZone : MonoBehaviour
{
    public PickaxeDigToolMaster masterTool;
    [Tooltip("つるはしが掘れるレイヤー（Terrainに加えてPickaxe専用レイヤーなどを設定）")]
    public LayerMask diggableLayers;
    
    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 後方互換のためTag判定も残しつつ、レイヤーマスクでも判定
        if (other.CompareTag("Terrain") || IsInLayerMask(other.gameObject.layer, diggableLayers))
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