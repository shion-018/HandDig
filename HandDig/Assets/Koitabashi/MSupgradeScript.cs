using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSupgradeScript : MonoBehaviour
{
    public bool isGetMStreasure = false;
    public VRPlayerMovement vrPlayerMovement;
    bool onlyOneTime = false;

    // isGetMStreasure��true�ɂȂ�����VRPlayerMovement��speed��{
    void Update()
    {
        if (isGetMStreasure && !onlyOneTime)
        {
            vrPlayerMovement.moveSpeed = vrPlayerMovement.moveSpeed * 2;
            onlyOneTime = true;
        }
    }
}
