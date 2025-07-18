using UnityEngine;

public class SwingReadyZone : MonoBehaviour
{
    public PickaxeDigTool pickaxeTool;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickaxe"))
        {
            pickaxeTool.SetSwingReady(true);
            Debug.Log("[SwingZone] 振りかぶり検出！");
        }
    }
}
