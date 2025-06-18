using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDigTool
{
    void OnTriggerEnter(Collider other);
    void OnTriggerExit(Collider other);
    void UpdateDig(Vector3 toolPosition);
}
