using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickaxeHitZone : MonoBehaviour
{
    public PickaxeDigToolMaster masterTool;

    private void OnTriggerEnter(Collider other)
    {
        // Terrainタグ + PickaxeOnlyタグの両方で判定
        if (other.CompareTag("Terrain") || other.CompareTag("PickaxeOnly"))
        {
            Debug.Log($"[PickaxeHitZone] 衝突: {other.name}, タグ: {other.tag}, 位置: {other.transform.position}");
            
            if (masterTool != null)
            {
                bool isSwingReady = masterTool.IsSwingReady();
                Debug.Log($"[PickaxeHitZone] isSwingReady: {isSwingReady}");

                if (isSwingReady)
                {
                    // Master側の新しいAPIに合わせて呼び出し
                    masterTool.OnMainHit(other);
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