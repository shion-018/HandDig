using UnityEngine;

public class SwingReadyZone : MonoBehaviour
{
    public PickaxeDigTool pickaxeTool;
    public PickaxeDigToolMaster pickaxeToolMaster;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickaxe"))
        {
            if (pickaxeTool != null)
            {
                pickaxeTool.SetSwingReady(true);
                Debug.Log("[SwingZone] ピッケルが検出！");
            }
            
            if (pickaxeToolMaster != null)
            {
                pickaxeToolMaster.SetSwingReady(true);
                Debug.Log("[SwingZone] ピッケルマスターが検出！");
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pickaxe"))
        {
            if (pickaxeTool != null)
            {
                pickaxeTool.SetSwingReady(false);
                Debug.Log("[SwingZone] ピッケルがゾーンから出ました");
            }
            
            if (pickaxeToolMaster != null)
            {
                pickaxeToolMaster.OnSwingZoneExit();
                Debug.Log("[SwingZone] ピッケルマスターがゾーンから出ました");
            }
        }
    }
}
