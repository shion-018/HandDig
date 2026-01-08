using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeHitZone : MonoBehaviour
{
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    public PickaxeDigToolMaster masterTool;

    private void OnTriggerEnter(Collider other)
    {
        // Terrainタグ + PickaxeOnlyタグの両方で判定
        if (other.CompareTag("Terrain") || other.CompareTag("PickaxeOnly"))
        {
            if (enableDebugLog) Debug.Log($"[PickaxeHitZone] 衝突: {other.name}, タグ: {other.tag}, 位置: {other.transform.position}");
            
            if (masterTool != null)
            {
                bool isSwingReady = masterTool.IsSwingReady();
                if (enableDebugLog) Debug.Log($"[PickaxeHitZone] isSwingReady: {isSwingReady}");

                if (isSwingReady)
                {
                    // Master側の新しいAPIに合わせて呼び出し
                    masterTool.OnMainHit(other);
                    if (enableDebugLog) Debug.Log("[PickaxeHitZone] 掘り実行！");
                }
                else
                {
                    if (enableDebugLog) Debug.Log("[PickaxeHitZone] 振りかぶり準備ができていません");
                }
            }
            else
            {
                Debug.LogError("[PickaxeHitZone] masterToolが設定されていません");
            }
        }
    }
}