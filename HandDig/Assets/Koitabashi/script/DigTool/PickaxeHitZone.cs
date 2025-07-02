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
            masterTool.OnAnyHit();
        }
    }
}