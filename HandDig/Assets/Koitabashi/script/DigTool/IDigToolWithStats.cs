using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDigToolWithStats : IDigTool
{
    void SetStats(DigToolStats stats, int upgradeLevel);
    void SetHandStats(HandDigStats stats, int upgradeLevel);
    void SetPickaxeStats(PickaxeDigStats stats, int upgradeLevel);
    void SetDrillStats(DrillDigStats stats, int upgradeLevel);
}
