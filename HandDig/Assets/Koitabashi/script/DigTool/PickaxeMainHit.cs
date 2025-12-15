using UnityEngine;

public class PickaxeMainHit : MonoBehaviour
{
    public PickaxeDigToolMaster masterTool;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Terrain") && !other.CompareTag("PickaxeOnly"))
            return;

        if (masterTool == null)
        {
            Debug.LogError("[PickaxeMainHit] masterTool Ç™ñ¢ê›íË");
            return;
        }

        if (!masterTool.IsSwingReady())
        {
            Debug.Log("[PickaxeMainHit] SwingReady Ç≈ÇÕÇ»Ç¢ÇΩÇﬂñ≥éã");
            return;
        }

        Debug.Log($"[PickaxeMainHit] Hit: {other.name}");
        masterTool.OnAnyHit();
    }
}
