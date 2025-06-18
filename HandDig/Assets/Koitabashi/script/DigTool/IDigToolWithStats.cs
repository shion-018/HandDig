using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDigToolWithStats : IDigTool
{
    void SetStats(DigToolStats stats, int upgradeLevel);
}
