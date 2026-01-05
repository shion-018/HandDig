using UnityEngine;

/// <summary>
/// 振りかぶりゾーン（SwingZone）
/// 仕様: ゾーン内にいる間、トリガーを押すと掘れるようになる
///       トリガー押しっぱなしならゾーンから出ても SwingReady を維持
/// </summary>
public class SwingReadyZone : MonoBehaviour
{
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = false;

    [Tooltip("PickaxeDigToolMaster用")]
    public PickaxeDigToolMaster masterTool;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Pickaxe"))
            return;

        CheckAndSetSwingReady();
    }

    void OnTriggerStay(Collider other)
    {
        // ゾーン内にいる間はいつでもトリガー押しで SwingReady にする
        if (!other.CompareTag("Pickaxe"))
            return;

        CheckAndSetSwingReady();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Pickaxe"))
            return;

        // トリガーが押されている場合は SwingReady を維持（false にしない）
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        if (isTriggerHeld || isSpaceHeld)
        {
            if (enableDebugLog) Debug.Log("[SwingZone] ゾーンから出ましたが、トリガー押しっぱなしのため SwingReady を維持");
            return; // SwingReady を維持
        }

        // トリガーが離れている場合は SwingReady をリセット
        if (masterTool != null)
        {
            masterTool.OnSwingZoneExit();
        }
    }

    private void CheckAndSetSwingReady()
    {
        // トリガー入力が押されているかチェック
        bool isTriggerHeld = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        bool isSpaceHeld = Input.GetKey(KeyCode.Space);

        if (!isTriggerHeld && !isSpaceHeld)
            return; // トリガーが押されていない場合は何もしない

        if (masterTool != null)
        {
            masterTool.OnSwingZoneEntered();
        }
    }
}
