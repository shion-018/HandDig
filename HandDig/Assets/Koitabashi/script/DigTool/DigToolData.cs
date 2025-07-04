using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DigToolData
{
    public string toolName;
    public DigToolStats stats;  // �X�N���v�^�u���I�u�W�F�N�g
    [Min(0)] public int currentUpgradeLevel = 0;  // ���݂̋����i�K
}
