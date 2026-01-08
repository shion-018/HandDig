using UnityEngine;

/// <summary>
/// つるはしのメインコライダー（地面に当たった時の判定）
/// 仕様: トリガー入力をしながらSwingZoneに入れた後、そのままトリガー入力をしながら地面に当てると掘りが発生
/// </summary>
public class PickaxeMainHit : MonoBehaviour
{
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    public PickaxeDigToolMaster masterTool;

    private void OnTriggerEnter(Collider other)
    {
        // 地形タグのチェック
        if (!other.CompareTag("Terrain") && !other.CompareTag("PickaxeOnly"))
            return;

        if (masterTool == null)
        {
            Debug.LogError("[PickaxeMainHit] masterTool が未設定");
            return;
        }

        // SwingReady状態のチェック
        if (!masterTool.IsSwingReady())
        {
            if (enableDebugLog) Debug.Log("[PickaxeMainHit] SwingReady ではないため無効");
            return;
        }

        // ★ 重要: トリガー入力が継続しているかチェック
        // 仕様: 「そのままトリガー入力をしながら地面にメインのコライダーを当てる」
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        if (!isTriggerHeld && !isSpaceHeld)
        {
            if (enableDebugLog) Debug.Log("[PickaxeMainHit] トリガーが押されていません");
            return;
        }

        if (enableDebugLog) Debug.Log($"[PickaxeMainHit] Hit: {other.name} (トリガー押下中)");
        masterTool.OnMainHit(other);
    }
}
