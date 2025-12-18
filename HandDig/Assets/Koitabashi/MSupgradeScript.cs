using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSupgradeScript : MonoBehaviour
{
    public bool isGetMStreasure = false;
    public VRPlayerMovement vrPlayerMovement;
    bool onlyoneTime = false;

    // isGetMStreasure‚ªtrue‚É‚È‚Á‚½‚çVRPlayerMovement‚Ìspeed“ñ”{
    void Update()
    {
        if (isGetMStreasure == true&&onlyoneTime==false)
        {
            vrPlayerMovement.moveSpeed = vrPlayerMovement.moveSpeed * 2;
            onlyoneTime = true;
        }
    }
}
